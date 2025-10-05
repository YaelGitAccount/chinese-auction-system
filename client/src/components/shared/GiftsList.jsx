import { useEffect, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Card } from "primereact/card";
import { Badge } from "primereact/badge";
import { Toast } from "primereact/toast";
import { useRef } from "react";
import api from "../../api/axios.js";
import { useAuth } from "../../context/AuthContext.jsx";

export default function GiftsList() {
  const [gifts, setGifts] = useState([]);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    loadGifts();
  }, []);

  const loadGifts = async () => {
    try {
      console.log("Loading available gifts from /api/Gift/available");
      const response = await api.get("/Gift/available");
      console.log("Available gifts loaded successfully:", response.data);
      setGifts(response.data);
    } catch (error) {
      console.error("Error loading gifts:", error);
      
      let errorMessage = "Unable to load gifts list";
      
      if (error.response) {
        console.error("Error response:", error.response);
        if (error.response.status === 404) {
          errorMessage = "Gifts service not found - check if server is running";
        } else if (error.response.status === 500) {
          errorMessage = "Server error";
        }
      } else if (error.request) {
        errorMessage = "Cannot connect to server - check connection";
        console.error("No response received:", error.request);
      }
      
      showToast("error", "Error", errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const addToCart = async (gift) => {
    if (!user) {
      showToast("warn", "Login Required", "You must login to add gifts to cart");
      return;
    }

    // Check if gift is available for purchase
    if (gift.isLotteryCompleted) {
      showToast("warn", "Cannot Purchase", "Cannot purchase this gift - lottery already drawn");
      return;
    }

    try {
      console.log("Adding to cart:", { giftId: gift.id, gift: gift });
      
      // Use correct server path: POST /api/Purchase/cart/{giftId}
      await api.post(`/Purchase/cart/${gift.id}`);
      
      showToast("success", "Success", `Gift "${gift.name}" added to cart successfully!`);
      
    } catch (error) {
      console.error("Error adding to cart:", error);
      
      let errorMessage = "Cannot add gift to cart";
      
      if (error.response) {
        console.error("Error response details:", {
          status: error.response.status,
          statusText: error.response.statusText,
          data: error.response.data
        });
        
        if (error.response.status === 403) {
          errorMessage = "Cannot add to cart - lottery already completed";
        } else if (error.response.status === 401) {
          errorMessage = "Unauthorized - please login again";
        } else if (error.response.status === 400) {
          errorMessage = "Cannot add this gift to cart";
        } else if (error.response.status === 404) {
          errorMessage = "Gift not found";
        } else if (error.response.status === 500) {
          errorMessage = "Server error - please try again later";
        }
      } else if (error.request) {
        errorMessage = "Cannot connect to server - check connection";
        console.error("No response received:", error.request);
      }
      
      showToast("error", "Error", errorMessage);
    }
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const priceBodyTemplate = (rowData) => {
    return (
      <span className="font-semibold text-lg">
        ${rowData.price?.toLocaleString()}
      </span>
    );
  };

  const actionBodyTemplate = (rowData) => {
    const isLotteryCompleted = rowData.isLotteryCompleted;
    const buttonLabel = isLotteryCompleted ? "Completed" : "Add to Cart";
    const buttonIcon = isLotteryCompleted ? "pi pi-trophy" : "pi pi-shopping-cart";
    const buttonClass = isLotteryCompleted ? "p-button-sm p-button-secondary" : "p-button-sm p-button-success";
    
    return (
      <Button
        icon={buttonIcon}
        label={buttonLabel}
        className={buttonClass}
        onClick={() => addToCart(rowData)}
        disabled={!user || isLotteryCompleted}
        tooltip={isLotteryCompleted ? "Gift lottery already completed - cannot purchase" : "Add gift to shopping cart"}
      />
    );
  };

  const statusBodyTemplate = (rowData) => {
    if (rowData.isLotteryCompleted) {
      return (
        <Badge 
          value="Completed" 
          severity="warning"
          icon="pi pi-trophy"
          className="status-badge status-completed"
        />
      );
    }
    return (
      <Badge 
        value="Available" 
        severity="success"
        icon="pi pi-check"
        className="status-badge status-available"
      />
    );
  };

  const categoryBodyTemplate = (rowData) => {
    const categoryName = rowData.categoryName || "Uncategorized";
    const categoryColor = rowData.categoryColor || "#6c757d";
    
    return (
      <Badge 
        value={categoryName} 
        style={{ backgroundColor: categoryColor, color: '#fff' }}
      />
    );
  };

  return (
    <div className="p-4">
      <Toast ref={toast} />
      
      <Card className="shadow-3">
        <div className="card-header">
          <div className="flex justify-content-between align-items-center">
            <h2 className="m-0 text-white">🎁 Available Gifts</h2>
            <Badge 
              value={`${gifts.length} gifts available`} 
              className="p-badge-lg"
              style={{ backgroundColor: 'rgba(255,255,255,0.2)', color: 'white' }}
            />
          </div>
        </div>

        {gifts.length === 0 && !loading ? (
          <div className="empty-state">
            <div className="text-6xl mb-3">🎁</div>
            <h3>No gifts available at the moment</h3>
            <p>Check back later for new exciting gifts!</p>
          </div>
        ) : (
          <DataTable 
            value={gifts} 
            paginator 
            rows={10} 
            loading={loading}
            responsiveLayout="scroll"
            emptyMessage="No gifts found"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} gifts"
            className="p-datatable-gridlines"
          >
            <Column 
              field="name" 
              header="Gift Name" 
              sortable 
              style={{ minWidth: '200px' }}
            />
            <Column 
              field="categoryName" 
              header="Category" 
              body={categoryBodyTemplate}
              sortable 
              style={{ minWidth: '120px' }}
            />
            <Column 
              field="donorName" 
              header="Donor" 
              sortable 
              style={{ minWidth: '150px' }}
            />
            <Column 
              field="price" 
              header="Ticket Price" 
              body={priceBodyTemplate}
              sortable 
              style={{ minWidth: '120px' }}
            />
            <Column 
              header="Status"
              body={statusBodyTemplate}
              style={{ width: '120px' }}
            />
            <Column 
              header="Actions"
              body={actionBodyTemplate}
              style={{ width: '160px' }}
            />
          </DataTable>
        )}
      </Card>
    </div>
  );
}
