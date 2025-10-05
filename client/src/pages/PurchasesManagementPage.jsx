import React, { useState, useEffect, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Badge } from "primereact/badge";
import { Toast } from "primereact/toast";
import { Button } from "primereact/button";
import { useAuth } from "../context/AuthContext.jsx";
import api from "../api/axios.js";

export default function PurchasesManagementPage() {
  const [purchases, setPurchases] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalRevenue, setTotalRevenue] = useState(0);
  const { user, isManager } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    if (user && isManager()) {
      loadPurchases();
    }
  }, [user]);

  const loadPurchases = async () => {
    try {
      setLoading(true);
      const response = await api.get("/Purchase/all");
      console.log("Purchase data received:", response.data);
      const purchaseData = response.data.data || response.data.items || response.data || [];
      setPurchases(purchaseData);
      calculateTotalRevenue(purchaseData);
    } catch (error) {
      console.error("Error loading purchases:", error);
      showToast("error", "Error", "Unable to load purchases data");
    } finally {
      setLoading(false);
    }
  };

  const calculateTotalRevenue = (purchasesList) => {
    const total = purchasesList.reduce((sum, item) => {
      return sum + (item.giftPrice || 0) * (item.totalQuantity || 0);
    }, 0);
    setTotalRevenue(total);
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const priceBodyTemplate = (rowData) => {
    return `$${rowData.giftPrice || 0}`;
  };

  const totalRevenueBodyTemplate = (rowData) => {
    const total = (rowData.giftPrice || 0) * (rowData.totalQuantity || 0);
    return `$${total}`;
  };

  const buyersBodyTemplate = (rowData) => {
    return (
      <Badge 
        value={`${rowData.buyersCount || 0} buyers`} 
        severity="info"
      />
    );
  };

  const quantityBodyTemplate = (rowData) => {
    return (
      <Badge 
        value={`${rowData.totalQuantity || 0} tickets`} 
        severity="success"
      />
    );
  };

  if (!user || !isManager()) {
    return (
      <div className="flex justify-content-center align-items-center p-4">
        <Card title="Access Restricted">
          <p>Only managers can access purchases management.</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-4">
      <Toast ref={toast} />
      
      <Card>
        <div className="flex justify-content-between align-items-center mb-4">
          <h2 className="m-0">All Purchases Management</h2>
          <div className="flex align-items-center gap-2">
            <Badge 
              value={`${purchases.length} gifts`} 
              className="p-badge-lg" 
            />
            <Badge 
              value={`Total Revenue: $${totalRevenue}`} 
              severity="success" 
              className="p-badge-lg"
            />
          </div>
        </div>

        {purchases.length === 0 && !loading ? (
          <div className="text-center p-4">
            <i className="pi pi-shopping-bag" style={{ fontSize: "3rem", color: "#ccc" }}></i>
            <h3>No Purchases</h3>
            <p>No purchases have been made yet.</p>
          </div>
        ) : (
          <DataTable 
            value={purchases} 
            loading={loading}
            paginator 
            rows={10}
            responsiveLayout="scroll"
            emptyMessage="No purchases found"
            sortField="giftName"
            sortOrder={1}
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} gifts"
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
              header="Total Tickets Sold" 
              body={quantityBodyTemplate}
              sortable 
            />
            <Column 
              field="buyersCount" 
              header="Number of Buyers" 
              body={buyersBodyTemplate}
              sortable 
            />
            <Column 
              header="Total Revenue" 
              body={totalRevenueBodyTemplate}
              sortable 
            />
          </DataTable>
        )}

        <div className="mt-4 text-center">
          <Button
            label="Refresh Data"
            icon="pi pi-refresh"
            className="p-button-outlined"
            onClick={loadPurchases}
            loading={loading}
          />
        </div>
      </Card>
    </div>
  );
}
