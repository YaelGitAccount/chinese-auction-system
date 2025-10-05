import React, { useState, useEffect, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Toast } from "primereact/toast";
import { Badge } from "primereact/badge";
import { Chip } from "primereact/chip";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { 
  getGiftsAwaitingLottery, 
  getManagerLotteryResults, 
  drawLottery, 
  getLotterySummary 
} from "../api/lottery.js";
import { useAuth } from "../context/AuthContext.jsx";

export default function LotteryManagementPage() {
  const [pendingGifts, setPendingGifts] = useState([]);
  const [completedLotteries, setCompletedLotteries] = useState([]);
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const { user, isManager } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    if (user && isManager()) {
      loadData();
    }
  }, [user]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [pending, completed, summaryData] = await Promise.all([
        getGiftsAwaitingLottery(),
        getManagerLotteryResults(),
        getLotterySummary()
      ]);
      
      setPendingGifts(pending);
      setCompletedLotteries(completed);
      setSummary(summaryData);
      console.log("Manager lottery summary:", summaryData); // Debug log
    } catch (error) {
      console.error("Error loading lottery data:", error);
      showToast("error", "Error", "Unable to load lottery data");
    } finally {
      setLoading(false);
    }
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const handleDrawLottery = (gift) => {
    confirmDialog({
      message: `Are you sure you want to draw the lottery for "${gift.name}"? This action cannot be undone.`,
      header: "Confirm Lottery Draw",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          setActionLoading(true);
          await drawLottery(gift.id);
          showToast("success", "Success", `Lottery for "${gift.name}" drawn successfully!`);
          loadData(); // Refresh data
        } catch (error) {
          console.error("Error drawing lottery:", error);
          let errorMessage = "Error drawing lottery";
          if (error.response?.status === 400) {
            errorMessage = "Cannot draw lottery for this gift - no participants or already drawn";
          }
          showToast("error", "Error", errorMessage);
        } finally {
          setActionLoading(false);
        }
      },
      acceptLabel: "Yes, Draw",
      rejectLabel: "Cancel",
      acceptClassName: "p-button-success"
    });
  };

  const priceBodyTemplate = (rowData) => {
    return `$${rowData.price || 0}`;
  };

  const participantsBodyTemplate = (rowData) => {
    const count = rowData.BuyersCount || rowData.buyersCount || 0;
    return (
      <Chip 
        label={`${count} participants`} 
        className={count > 0 ? "p-chip-success" : "p-chip-warning"} 
      />
    );
  };

  const completedParticipantsBodyTemplate = (rowData) => {
    const count = rowData.participantsCount || 0;
    return (
      <Chip 
        label={`${count} participants`} 
        className="p-chip-success" 
      />
    );
  };

  const actionBodyTemplate = (rowData) => {
    const hasParticipants = (rowData.BuyersCount || rowData.buyersCount || 0) > 0;
    
    return (
      <Button
        icon="pi pi-star"
        label="Draw"
        className="p-button-sm p-button-success"
        onClick={() => handleDrawLottery(rowData)}
        disabled={!hasParticipants || actionLoading}
        tooltip={!hasParticipants ? "No participants for this gift" : "Draw lottery for this gift"}
      />
    );
  };

  const categoryBodyTemplate = (rowData) => {
    return (
      <Badge 
        value={rowData.categoryName || "Undefined"} 
        severity="info"
      />
    );
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

  if (!user || !isManager()) {
    return (
      <div className="flex justify-content-center align-items-center p-4">
        <Card title="Access Restricted">
          <p>Only managers can access lottery management.</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-4">
      <Toast ref={toast} />
      <ConfirmDialog />
      
      {/* Lottery Summary */}
      {summary && (
        <Card className="mb-4">
          <div className="grid">
            <div className="col-12">
              <h3 className="mt-0">Lottery Summary</h3>
            </div>
            <div className="col-3">
              <div className="text-center p-3 bg-blue-50 border-round">
                <div className="text-2xl font-bold text-blue-600">{summary.totalGifts || 0}</div>
                <div className="text-600">Total Gifts</div>
              </div>
            </div>
            <div className="col-3">
              <div className="text-center p-3 bg-green-50 border-round">
                <div className="text-2xl font-bold text-green-600">{summary.completedLotteries || 0}</div>
                <div className="text-600">Completed Lotteries</div>
              </div>
            </div>
            <div className="col-3">
              <div className="text-center p-3 bg-orange-50 border-round">
                <div className="text-2xl font-bold text-orange-600">{summary.awaitingLotteries || 0}</div>
                <div className="text-600">Awaiting Lottery</div>
              </div>
            </div>
            <div className="col-3">
              <div className="text-center p-3 bg-purple-50 border-round">
                <div className="text-2xl font-bold text-purple-600">${summary.totalRevenue || 0}</div>
                <div className="text-600">Total Revenue</div>
              </div>
            </div>
          </div>
        </Card>
      )}

      {/* Gifts Awaiting Lottery */}
      <Card className="mb-4">
        <div className="flex justify-content-between align-items-center mb-4">
          <h3 className="m-0">Gifts Awaiting Lottery</h3>
          <Badge value={`${pendingGifts.length} gifts`} className="p-badge-lg" />
        </div>

        {pendingGifts.length === 0 ? (
          <div className="text-center p-4">
            <i className="pi pi-check-circle" style={{ fontSize: "3rem", color: "#4caf50" }}></i>
            <h4>All gifts have been drawn!</h4>
            <p>No gifts are currently awaiting lottery draw.</p>
          </div>
        ) : (
          <DataTable 
            value={pendingGifts} 
            loading={loading}
            responsiveLayout="scroll"
            emptyMessage="No gifts awaiting lottery"
          >
            <Column field="name" header="Gift Name" sortable />
            <Column 
              field="categoryName" 
              header="Category" 
              body={categoryBodyTemplate}
              sortable 
            />
            <Column field="donorName" header="Donor" sortable />
            <Column 
              field="price" 
              header="Ticket Price" 
              body={priceBodyTemplate}
              sortable 
            />
            <Column 
              field="BuyersCount" 
              header="Participants" 
              body={participantsBodyTemplate}
              sortable 
            />
            <Column 
              header="Actions"
              body={actionBodyTemplate}
              style={{ width: '150px' }}
            />
          </DataTable>
        )}
      </Card>

      {/* Completed Lotteries */}
      <Card>
        <div className="flex justify-content-between align-items-center mb-4">
          <h3 className="m-0">Completed Lotteries</h3>
          <Badge value={`${completedLotteries.length} results`} className="p-badge-lg" severity="success" />
        </div>

        {completedLotteries.length === 0 ? (
          <div className="text-center p-4">
            <i className="pi pi-clock" style={{ fontSize: "3rem", color: "#ccc" }}></i>
            <h4>No completed lotteries</h4>
            <p>Lotteries you draw will appear here.</p>
          </div>
        ) : (
          <DataTable 
            value={completedLotteries} 
            loading={loading}
            paginator 
            rows={10}
            responsiveLayout="scroll"
            emptyMessage="No completed lotteries"
            sortField="drawDate"
            sortOrder={-1}
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} results"
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
              body={completedParticipantsBodyTemplate}
              sortable 
            />
            <Column 
              field="drawDate" 
              header="Draw Date" 
              body={dateBodyTemplate}
              sortable 
            />
          </DataTable>
        )}
      </Card>
    </div>
  );
}
