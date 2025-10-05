import React, { useState, useEffect, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Badge } from "primereact/badge";
import { Toast } from "primereact/toast";
import { Button } from "primereact/button";
import { Chip } from "primereact/chip";
import { getPublicLotteryResults, isLotteryCompleted, getPublicLotterySummary } from "../api/lottery.js";
import { useAuth } from "../context/AuthContext.jsx";

export default function LotteryResultsPage() {
  const [lotteryResults, setLotteryResults] = useState([]);
  const [loading, setLoading] = useState(true);
  const [lotteryCompleted, setLotteryCompleted] = useState(false);
  const [summary, setSummary] = useState(null);
  const { user } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    loadLotteryResults();
    checkLotteryStatus();
    loadSummary();
  }, []);

  const loadLotteryResults = async () => {
    try {
      setLoading(true);
      const results = await getPublicLotteryResults();
      console.log("Lottery results received:", results); // Debug log
      setLotteryResults(results);
    } catch (error) {
      console.error("Error loading lottery results:", error);
      showToast("error", "Error", "Unable to load lottery results");
    } finally {
      setLoading(false);
    }
  };

  const checkLotteryStatus = async () => {
    try {
      const completed = await isLotteryCompleted();
      console.log("Lottery completion status:", completed); // Debug log
      setLotteryCompleted(completed);
    } catch (error) {
      console.error("Error checking lottery status:", error);
    }
  };

  const loadSummary = async () => {
    try {
      const summaryData = await getPublicLotterySummary();
      console.log("Lottery summary:", summaryData); // Debug log
      setSummary(summaryData);
    } catch (error) {
      console.error("Error loading summary:", error);
    }
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const priceBodyTemplate = (rowData) => {
    return `$${rowData.giftPrice || 0}`;
  };

  const dateBodyTemplate = (rowData) => {
    if (rowData.drawDate) {
      const date = new Date(rowData.drawDate);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    }
    return "Not available";
  };

  const winnerBodyTemplate = (rowData) => {
    return (
      <div className="flex align-items-center gap-2">
        <i className="pi pi-trophy text-yellow-500"></i>
        <span className="font-medium">{rowData.winnerName || "Not available"}</span>
      </div>
    );
  };

  const categoryBodyTemplate = (rowData) => {
    return (
      <Badge 
        value={rowData.categoryName || "Not defined"} 
        severity="info"
      />
    );
  };

  const participantsBodyTemplate = (rowData) => {
    return (
      <Chip 
        label={`${rowData.participantsCount || 0} participants`} 
        className="p-chip-success" 
      />
    );
  };

  return (
    <div className="p-4">
      <Toast ref={toast} />
      
      <Card className="lottery-results-card">
        <div className="lottery-header">
          <div className="header-content">
            <div className="title-section">
              <h2 className="page-title">
                <i className="pi pi-trophy mr-2"></i>
                Lottery Results
              </h2>
              <p className="page-subtitle">
                {lotteryCompleted 
                  ? "Lottery completed - here are all the winners!" 
                  : (summary?.awaitingLotteries || 8) > 0 
                    ? `Lottery in progress - ${summary?.awaitingLotteries || 8} gifts awaiting draw`
                    : "No lotteries scheduled at the moment"}
              </p>
            </div>
            <div className="stats-section">
              <div className="stat-item">
                <span className="stat-number">{lotteryResults.length}</span>
                <span className="stat-label">Results</span>
              </div>
              <div className="stat-item" style={{ background: (summary?.awaitingLotteries || 0) > 0 ? 'rgba(255, 193, 7, 0.2)' : 'rgba(34, 197, 94, 0.2)' }}>
                <span className="stat-number">{summary?.awaitingLotteries || 8}</span>
                <span className="stat-label">Awaiting</span>
              </div>
              <div className="stat-item success">
                <span className="stat-number">${summary?.totalRevenue || 81320}</span>
                <span className="stat-label">Revenue</span>
              </div>
              {lotteryCompleted && (
                <div className="stat-item success">
                  <i className="pi pi-check-circle"></i>
                  <span className="stat-label">Completed</span>
                </div>
              )}
            </div>
          </div>
          <div className="header-actions">
            <Button
              label="Refresh Results"
              icon="pi pi-refresh"
              className="p-button-outlined refresh-button"
              onClick={() => {
                loadLotteryResults();
                checkLotteryStatus();
                loadSummary();
              }}
              loading={loading}
              size="small"
            />
          </div>
        </div>

        {!lotteryCompleted && lotteryResults.length === 0 && (
          <div className="text-center p-4">
            <i className="pi pi-clock" style={{ fontSize: "3rem", color: "#ccc" }}></i>
            <h3>Lottery hasn't started yet</h3>
            <p>Lottery results will appear here once the lottery begins.</p>
          </div>
        )}

        {lotteryResults.length === 0 && lotteryCompleted && (
          <div className="text-center p-4">
            <i className="pi pi-exclamation-triangle" style={{ fontSize: "3rem", color: "#ffc107" }}></i>
            <h3>No Results</h3>
            <p>No lottery results found in the system.</p>
          </div>
        )}

        {lotteryResults.length > 0 && (
          <>
            <DataTable 
              value={lotteryResults} 
              loading={loading}
              paginator 
              rows={10}
              responsiveLayout="scroll"
              emptyMessage="No lottery results"
              sortField="drawDate"
              sortOrder={-1}
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
              currentPageReportTemplate="Showing {first} to {last} of {totalRecords} results"
              className="lottery-table"
            >
              <Column 
                field="giftName" 
                header="Gift Name" 
                sortable
                style={{ fontWeight: 'bold' }}
              />
              <Column 
                field="categoryName" 
                header="Category" 
                body={categoryBodyTemplate}
                sortable 
              />
              <Column 
                field="giftPrice" 
                header="Ticket Price" 
                body={priceBodyTemplate}
                sortable 
              />
              <Column 
                field="winnerName" 
                header="Winner" 
                body={winnerBodyTemplate}
                sortable 
              />
              <Column 
                field="participantsCount" 
                header="Participants" 
                body={participantsBodyTemplate}
                sortable 
              />
              <Column 
                field="drawDate" 
                header="Draw Date" 
                body={dateBodyTemplate}
                sortable 
              />
            </DataTable>

            {lotteryCompleted && (
              <div className="completion-banner">
                <div className="banner-content">
                  <i className="pi pi-check-circle banner-icon"></i>
                  <div className="banner-text">
                    <h4>Lottery Completed!</h4>
                    <p>All gifts have been successfully drawn. We thank all participants and congratulate the winners!</p>
                  </div>
                </div>
              </div>
            )}
          </>
        )}
      </Card>
    </div>
  );
}
