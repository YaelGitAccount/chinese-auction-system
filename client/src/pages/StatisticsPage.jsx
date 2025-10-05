import React, { useEffect, useState, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Toast } from "primereact/toast";
import { Badge } from "primereact/badge";
import { Panel } from "primereact/panel";
import { useAuth } from "../context/AuthContext.jsx";
import api from "../api/axios";

export default function StatisticsPage() {
  const [overallStats, setOverallStats] = useState(null);
  const [giftStats, setGiftStats] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const { user, isManager } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    if (user && isManager()) {
      fetchAllData();
    }
  }, [user]);

  const fetchAllData = async () => {
    setLoading(true);
    try {
      await Promise.all([
        fetchOverallStats(),
        fetchGiftStats()
      ]);
    } catch (e) {
      setError("Error loading statistics");
      showToast("error", "Error", "Unable to load statistics data");
    } finally {
      setLoading(false);
    }
  };

  const fetchOverallStats = async () => {
    try {
      const { data } = await api.get("/Purchase/summary/overall");
      setOverallStats(data);
    } catch (e) {
      console.error("Error fetching overall stats:", e);
    }
  };

  const fetchGiftStats = async () => {
    try {
      const { data } = await api.get("/Purchase/summary/by-gift");
      setGiftStats(data || []);
    } catch (e) {
      console.error("Error fetching gift stats:", e);
    }
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const exportToCSV = () => {
    if (!giftStats.length) return;
    
    const csvContent = [
      "Gift Name,Ticket Price,Total Tickets Sold,Total Revenue,Number of Buyers",
      ...giftStats.map(item => 
        `"${item.giftName || 'N/A'}",${item.giftPrice || 0},${item.totalQuantity || 0},${(item.giftPrice || 0) * (item.totalQuantity || 0)},${item.buyersCount || 0}`
      )
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `auction_report_${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    showToast("success", "Success", "Report exported successfully");
  };

  const printReport = () => {
    window.print();
    showToast("info", "Print", "Print dialog opened");
  };

  const priceBodyTemplate = (rowData) => {
    return `$${rowData.giftPrice || 0}`;
  };

  const revenueBodyTemplate = (rowData) => {
    const revenue = (rowData.giftPrice || 0) * (rowData.totalQuantity || 0);
    return `$${revenue}`;
  };

  const quantityBodyTemplate = (rowData) => {
    return (
      <Badge 
        value={`${rowData.totalQuantity || 0} tickets`} 
        severity="success"
      />
    );
  };

  const buyersBodyTemplate = (rowData) => {
    return (
      <Badge 
        value={`${rowData.buyersCount || 0} buyers`} 
        severity="info"
      />
    );
  };

  if (!user || !isManager()) {
    return (
      <div className="flex justify-content-center align-items-center p-4">
        <Card title="Access Restricted">
          <p>Only managers can access reports and statistics.</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-4">
      <Toast ref={toast} />
      
      {/* Header */}
      <div className="flex justify-content-between align-items-center mb-4">
        <h1 className="m-0">Auction Reports & Statistics</h1>
        <div className="flex gap-2">
          <Button
            label="Export CSV"
            icon="pi pi-download"
            className="p-button-success"
            onClick={exportToCSV}
            disabled={!giftStats.length}
          />
          <Button
            label="Print Report"
            icon="pi pi-print"
            className="p-button-info"
            onClick={printReport}
          />
          <Button
            label="Refresh Data"
            icon="pi pi-refresh"
            className="p-button-outlined"
            onClick={fetchAllData}
            loading={loading}
          />
        </div>
      </div>

      {error && (
        <Card className="mb-4">
          <div className="text-red-500">{error}</div>
        </Card>
      )}

      {/* Overall Statistics */}
      {overallStats && (
        <Panel header="Overall Statistics" className="mb-4">
          <div className="grid">
            <div className="col-12 md:col-3">
              <div className="stats-card text-center">
                <div className="text-2xl font-bold text-blue-600">${overallStats.totalIncome || 0}</div>
                <div className="text-sm text-600">Total Revenue</div>
              </div>
            </div>
            <div className="col-12 md:col-3">
              <div className="stats-card text-center">
                <div className="text-2xl font-bold text-green-600">{overallStats.totalPurchases || 0}</div>
                <div className="text-sm text-600">Total Purchases</div>
              </div>
            </div>
            <div className="col-12 md:col-3">
              <div className="stats-card text-center">
                <div className="text-2xl font-bold text-purple-600">{overallStats.totalUsers || 0}</div>
                <div className="text-sm text-600">Total Participants</div>
              </div>
            </div>
            <div className="col-12 md:col-3">
              <div className="stats-card text-center">
                <div className="text-2xl font-bold text-orange-600">{overallStats.totalGifts || 0}</div>
                <div className="text-sm text-600">Total Gifts</div>
              </div>
            </div>
          </div>
        </Panel>
      )}

      {/* Detailed Gift Statistics */}
      <Card>
        <div className="flex justify-content-between align-items-center mb-4">
          <h3 className="m-0">Gift Performance Report</h3>
          <Badge 
            value={`${giftStats.length} gifts analyzed`} 
            severity="info"
          />
        </div>

        {giftStats.length === 0 && !loading ? (
          <div className="text-center p-4">
            <i className="pi pi-chart-bar" style={{ fontSize: "3rem", color: "#ccc" }}></i>
            <h3>No Data Available</h3>
            <p>No gift statistics found.</p>
          </div>
        ) : (
          <DataTable 
            value={giftStats} 
            loading={loading}
            paginator 
            rows={15}
            responsiveLayout="scroll"
            emptyMessage="No statistics available"
            sortField="totalQuantity"
            sortOrder={-1}
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} gifts"
            className="p-datatable-sm"
          >
            <Column 
              field="giftName" 
              header="Gift Name" 
              sortable
              style={{ fontWeight: 'bold' }}
            />
            <Column 
              field="giftPrice" 
              header="Ticket Price" 
              body={priceBodyTemplate}
              sortable 
            />
            <Column 
              field="totalQuantity" 
              header="Tickets Sold" 
              body={quantityBodyTemplate}
              sortable 
            />
            <Column 
              field="buyersCount" 
              header="Buyers" 
              body={buyersBodyTemplate}
              sortable 
            />
            <Column 
              header="Total Revenue" 
              body={revenueBodyTemplate}
              sortable 
            />
          </DataTable>
        )}
      </Card>
    </div>
  );
}
