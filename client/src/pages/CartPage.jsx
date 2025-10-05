import React, { useState, useEffect, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Toast } from "primereact/toast";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import api from "../api/axios.js";
import { useAuth } from "../context/AuthContext.jsx";

export default function CartPage() {
  const [cartItems, setCartItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalAmount, setTotalAmount] = useState(0);
  const { user } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    if (user) {
      loadCartItems();
    }
  }, [user]);

  const loadCartItems = async () => {
    try {
      console.log("Loading cart items from /api/Purchase/cart");
      
      // Use the correct path from server: GET /api/Purchase/cart
      const response = await api.get("/Purchase/cart");
      
      console.log("Cart items loaded successfully:", response.data);
      setCartItems(response.data || []);
      calculateTotal(response.data || []);
      
    } catch (error) {
      console.error("Error loading cart:", error);
      
      let errorMessage = "Unable to load cart";
      
      if (error.response) {
        console.error("Error response:", error.response);
        if (error.response.status === 401) {
          errorMessage = "Unauthorized - please log in again";
        } else if (error.response.status === 404) {
          errorMessage = "Service not found";
        } else if (error.response.status === 500) {
          errorMessage = "Server error";
        }
      } else if (error.request) {
        errorMessage = "Unable to connect to server";
        console.error("No response received:", error.request);
      }
      
      showToast("error", "Error", errorMessage);
      setCartItems([]); // Set empty cart in case of error
    } finally {
      setLoading(false);
    }
  };

  const calculateTotal = (items) => {
    const total = items.reduce((sum, item) => sum + (item.giftPrice || 0) * (item.quantity || 1), 0);
    setTotalAmount(total);
  };

  const removeFromCart = async (purchaseId) => {
    try {
      await api.delete(`/Purchase/cart/${purchaseId}`);
      showToast("success", "Success", "Gift removed from cart");
      loadCartItems(); // Refresh cart
    } catch (error) {
      console.error("Error removing from cart:", error);
      let errorMessage = "Unable to remove gift from cart";
      if (error.response) {
        if (error.response.status === 403) {
          errorMessage = "Cannot remove - lottery has already taken place";
        } else if (error.response.status === 400) {
          errorMessage = "Item not found or doesn't belong to you";
        }
      }
      showToast("error", "Error", errorMessage);
    }
  };

  const updateQuantity = async (purchaseId, action) => {
    try {
      await api.put(`/Purchase/cart/${purchaseId}/quantity?action=${action}`);
      showToast("success", "Success", "Quantity updated");
      loadCartItems(); // Refresh cart
    } catch (error) {
      console.error("Error updating quantity:", error);
      let errorMessage = "Unable to update quantity";
      if (error.response) {
        if (error.response.status === 403) {
          errorMessage = "Cannot update - lottery has already taken place";
        } else if (error.response.status === 400) {
          const responseText = error.response.data || error.response.statusText;
          if (responseText.includes("below 1")) {
            errorMessage = "Cannot reduce below 1. Use remove instead";
          } else {
            errorMessage = "Item not found or invalid action";
          }
        } else if (error.response.status === 404) {
          errorMessage = "Item not found or doesn't belong to you";
        }
      }
      showToast("error", "Error", errorMessage);
    }
  };

  const confirmRemove = (purchaseId, giftName) => {
    confirmDialog({
      message: `Are you sure you want to remove "${giftName}" from the cart?`,
      header: "Confirm Removal",
      icon: "pi pi-exclamation-triangle",
      accept: () => removeFromCart(purchaseId),
      acceptLabel: "Yes, Remove",
      rejectLabel: "Cancel",
      acceptClassName: "p-button-danger"
    });
  };

  const completePurchase = async () => {
    try {
      console.log("Completing purchase with /api/Purchase/checkout");
      
      // Use the correct path from server: POST /api/Purchase/checkout
      await api.post("/Purchase/checkout");
      
      showToast("success", "Success", "Purchase completed successfully!");
      
      // Refresh cart after purchase
      await loadCartItems();
      
    } catch (error) {
      console.error("Error completing purchase:", error);
      
      let errorMessage = "Unable to complete purchase";
      
      if (error.response) {
        if (error.response.status === 403) {
          errorMessage = "Cannot make purchase - lottery has already taken place";
        } else if (error.response.status === 400) {
          errorMessage = "No items in cart or purchase error";
        } else if (error.response.status === 401) {
          errorMessage = "Unauthorized - please log in again";
        } else if (error.response.status === 500) {
          errorMessage = "Server error - please try again later";
        }
      }
      
      showToast("error", "Error", errorMessage);
    }
  };

  const confirmPurchase = () => {
    confirmDialog({
      message: `Are you sure you want to make a purchase for $${totalAmount}?`,
      header: 'Confirm Purchase',
      icon: 'pi pi-question-circle',
      acceptLabel: 'Confirm',
      rejectLabel: 'Cancel',
      accept: completePurchase
    });
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

  const quantityBodyTemplate = (rowData) => {
    return (
      <div className="flex align-items-center gap-2">
        <Button 
          icon="pi pi-minus" 
          className="p-button-rounded p-button-sm p-button-outlined"
          onClick={() => updateQuantity(rowData.id, "decrease")}
          disabled={rowData.quantity <= 1}
          size="small"
          tooltip="Decrease quantity"
        />
        <span className="px-2 font-bold">{rowData.quantity || 1}</span>
        <Button 
          icon="pi pi-plus" 
          className="p-button-rounded p-button-sm p-button-outlined"
          onClick={() => updateQuantity(rowData.id, "increase")}
          size="small"
          tooltip="Increase quantity"
        />
      </div>
    );
  };

  const actionBodyTemplate = (rowData) => {
    return (
      <Button
        icon="pi pi-trash"
        className="p-button-rounded p-button-danger p-button-sm"
        onClick={() => confirmRemove(rowData.id, rowData.giftName)}
        tooltip="Remove from cart"
      />
    );
  };

  const giftNameBodyTemplate = (rowData) => {
    return rowData.giftName || "Not available";
  };

  if (!user) {
    return (
      <div className="flex justify-content-center align-items-center p-4">
        <Card title="Access Required">
          <p>You must be logged in to view your shopping cart.</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-4">
      <Toast ref={toast} />
      <ConfirmDialog />
      
      <Card>
        <div className="flex justify-content-between align-items-center mb-4">
          <h2 className="m-0">My Cart</h2>
          <div className="flex align-items-center gap-2">
            <span className="p-badge p-badge-lg">{cartItems.length} items</span>
            <span className="p-badge p-badge-success p-badge-lg">Total: ${totalAmount}</span>
          </div>
        </div>

        {cartItems.length === 0 && !loading ? (
          <div className="text-center p-4">
            <i className="pi pi-shopping-cart" style={{ fontSize: "3rem", color: "#ccc" }}></i>
            <h3>Cart is Empty</h3>
            <p>You haven't added any gifts to your cart yet.</p>
          </div>
        ) : (
          <DataTable 
            value={cartItems} 
            loading={loading}
            responsiveLayout="scroll"
            emptyMessage="Cart is empty"
          >
            <Column 
              field="giftName" 
              header="Gift Name" 
              body={giftNameBodyTemplate}
            />
            <Column 
              field="giftPrice" 
              header="Ticket Price" 
              body={priceBodyTemplate}
            />
            <Column 
              field="quantity" 
              header="Quantity"
              body={quantityBodyTemplate}
              style={{ width: '150px' }}
            />
            <Column 
              header="Total" 
              body={totalBodyTemplate}
            />
            <Column 
              header="Actions"
              body={actionBodyTemplate}
              style={{ width: '100px' }}
            />
          </DataTable>
        )}

        {cartItems.length > 0 && (
          <div className="flex justify-content-end mt-4">
            <Button 
              label="Proceed to Checkout" 
              icon="pi pi-credit-card"
              className="p-button-lg p-button-success"
              disabled={cartItems.length === 0}
              onClick={confirmPurchase}
            />
          </div>
        )}
      </Card>
    </div>
  );
}
