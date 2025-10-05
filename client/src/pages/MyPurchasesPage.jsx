import React, { useState, useEffect, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Badge } from "primereact/badge";
import { Toast } from "primereact/toast";
import { Chip } from "primereact/chip";
import api from "../api/axios.js";
import { useAuth } from "../context/AuthContext.jsx";

export default function MyPurchasesPage() {
  const [purchases, setPurchases] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalSpent, setTotalSpent] = useState(0);
  const { user } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    if (user) {
      loadPurchases();
    }
  }, [user]);

  const loadPurchases = async () => {
    try {
      const response = await api.get(`/Purchase/orders`);
      setPurchases(response.data);
      calculateTotal(response.data);
    } catch (error) {
      console.error("Error loading purchases:", error);
      showToast("error", "Error", "Unable to load purchase history");
    } finally {
      setLoading(false);
    }
  };

  const calculateTotal = (purchaseList) => {
    const total = purchaseList.reduce((sum, purchase) => {
      return sum + (purchase.giftPrice || 0) * (purchase.quantity || 1);
    }, 0);
    setTotalSpent(total);
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const priceBodyTemplate = (rowData) => {
    return `$${rowData.giftPrice || 0}`;
  };

  const totalBodyTemplate = (rowData) => {
    const total = (rowData.giftPrice || 0) * (rowData.quantity || 1);
    return `$${total}`;
  };

  const giftNameBodyTemplate = (rowData) => {
    return rowData.giftName || "Not available";
  };

  const statusBodyTemplate = (rowData) => {
    // If the gift was drawn in lottery
    if (rowData.isGiftLotteryCompleted) {
      return <Chip label="Lottery Completed" className="p-chip-warning" icon="pi pi-trophy" />;
    }
    
    // Purchase status
    const status = rowData.status || "Active";
    const severity = status === "Active" ? "success" : 
                    status === "Pending" ? "warning" : "secondary";
    
    return <Chip label={status} className={`p-chip-${severity}`} />;
  };

  const dateBodyTemplate = (rowData) => {
    if (rowData.purchaseDate) {
      const date = new Date(rowData.purchaseDate);
      return date.toLocaleDateString('en-US');
    }
    return "Not available";
  };

  if (!user) {
    return (
      <div className="flex justify-content-center align-items-center p-4">
        <Card title="Access Required">
          <p>You must be logged in to view your purchases.</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-4">
      <Toast ref={toast} />
      
      <Card>
        <div className="flex justify-content-between align-items-center mb-4">
          <h2 className="m-0">My Purchases</h2>
          <div className="flex align-items-center gap-2">
            <Badge value={`${purchases.length} purchases`} className="p-badge-lg" />
            <Badge value={`Total spent: $${totalSpent}`} severity="warning" className="p-badge-lg" />
          </div>
        </div>

        {purchases.length === 0 && !loading ? (
          <div className="text-center p-4">
            <i className="pi pi-list" style={{ fontSize: "3rem", color: "#ccc" }}></i>
            <h3>No Purchases</h3>
            <p>You haven't made any purchases in the Chinese auction yet.</p>
          </div>
        ) : (
          <DataTable 
            value={purchases} 
            loading={loading}
            paginator 
            rows={10}
            responsiveLayout="scroll"
            emptyMessage="No purchases"
            sortField="purchaseDate"
            sortOrder={-1}
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} purchases"
          >
            <Column 
              field="giftName" 
              header="Gift Name" 
              body={giftNameBodyTemplate}
              sortable
            />
            <Column 
              field="giftPrice" 
              header="Ticket Price" 
              body={priceBodyTemplate}
              sortable
            />
            <Column 
              field="quantity" 
              header="Quantity"
              sortable
            />
            <Column 
              header="Total" 
              body={totalBodyTemplate}
            />
            <Column 
              field="purchaseDate" 
              header="Purchase Date" 
              body={dateBodyTemplate}
              sortable
            />
            <Column 
              field="status" 
              header="Status" 
              body={statusBodyTemplate}
            />
          </DataTable>
        )}

        {purchases.length > 0 && (
          <div className="mt-4 p-3 bg-blue-50 border-round">
            <h4 className="mt-0">Summary:</h4>
            <div className="grid">
              <div className="col-6">
                <strong>Total purchases:</strong> {purchases.length}
              </div>
              <div className="col-6">
                <strong>Total spent:</strong> ${totalSpent}
              </div>
            </div>
          </div>
        )}
      </Card>
    </div>
  );
}