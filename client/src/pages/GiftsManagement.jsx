import React, { useState, useEffect, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { InputNumber } from "primereact/inputnumber";
import { Dropdown } from "primereact/dropdown";
import { Toast } from "primereact/toast";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { Badge } from "primereact/badge";
import { useAuth } from "../context/AuthContext.jsx";
import api from "../api/axios.js";
import * as categoriesApi from "../api/categories.js";

export default function GiftsManagement() {
  const [gifts, setGifts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [donors, setDonors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showDialog, setShowDialog] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [selectedGift, setSelectedGift] = useState(null);
  const [giftForm, setGiftForm] = useState({
    name: "",
    categoryId: null,
    donorId: null,
    price: 0
  });
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
      await Promise.all([
        loadGifts(),
        loadCategories(),
        loadDonors()
      ]);
    } finally {
      setLoading(false);
    }
  };

  const loadGifts = async () => {
    try {
      // For managers, we want to see all gifts, including those that were drawn
      const response = await api.get("/Gift");
      setGifts(response.data);
    } catch (error) {
      console.error("Error loading gifts:", error);
      showToast("error", "Error", "Unable to load gifts list");
    }
  };

  const loadCategories = async () => {
    try {
      const data = await categoriesApi.getActiveCategories();
      setCategories(data);
    } catch (error) {
      console.error("Error loading categories:", error);
      showToast("error", "Error", "Unable to load categories list");
    }
  };

  const loadDonors = async () => {
    try {
      const response = await api.get("/Donor");
      setDonors(response.data);
    } catch (error) {
      console.error("Error loading donors:", error);
      showToast("error", "Error", "Unable to load donors list");
    }
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const openNewDialog = () => {
    setGiftForm({
      name: "",
      categoryId: null,
      donorId: null,
      price: 0
    });
    setIsEditing(false);
    setSelectedGift(null);
    setShowDialog(true);
  };

  const openEditDialog = (gift) => {
    setGiftForm({
      name: gift.name,
      categoryId: gift.categoryId,
      donorId: gift.donorId,
      price: gift.price
    });
    setIsEditing(true);
    setSelectedGift(gift);
    setShowDialog(true);
  };

  const hideDialog = () => {
    setShowDialog(false);
    setGiftForm({
      name: "",
      categoryId: null,
      donorId: null,
      price: 0
    });
    setSelectedGift(null);
    setIsEditing(false);
  };

  const saveGift = async () => {
    try {
      if (!giftForm.name?.trim()) {
        showToast("warn", "Warning", "Gift name is required");
        return;
      }
      if (!giftForm.categoryId) {
        showToast("warn", "Warning", "Please select a category");
        return;
      }
      if (!giftForm.donorId) {
        showToast("warn", "Warning", "Please select a donor");
        return;
      }
      if (giftForm.price <= 0) {
        showToast("warn", "Warning", "Price must be greater than 0");
        return;
      }

      if (isEditing) {
        await api.put(`/Gift/${selectedGift.id}`, giftForm);
        showToast("success", "Success", "Gift updated successfully");
      } else {
        await api.post("/Gift", giftForm);
        showToast("success", "Success", "Gift created successfully");
      }

      hideDialog();
      loadGifts();
    } catch (error) {
      console.error("Error saving gift:", error);
      let errorMessage = "Error saving gift";
      if (error.response?.status === 400) {
        errorMessage = "Invalid data or validation error";
      }
      showToast("error", "Error", errorMessage);
    }
  };

  const deleteGift = async (gift) => {
    confirmDialog({
      message: `Are you sure you want to delete the gift "${gift.name}"?`,
      header: "Confirm Deletion",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          await api.delete(`/Gift/${gift.id}`);
          showToast("success", "Success", "Gift deleted successfully");
          loadGifts();
        } catch (error) {
          console.error("Error deleting gift:", error);
          let errorMessage = "Error deleting gift";
          if (error.response?.status === 400) {
            errorMessage = "Cannot delete gift that has already been purchased";
          }
          showToast("error", "Error", errorMessage);
        }
      },
      acceptLabel: "Yes, Delete",
      rejectLabel: "Cancel",
      acceptClassName: "p-button-danger"
    });
  };

  const categoryBodyTemplate = (rowData) => {
    const categoryName = rowData.categoryName || "Undefined";
    const categoryColor = rowData.categoryColor || "#6c757d";
    
    return (
      <Badge 
        value={categoryName} 
        style={{ backgroundColor: categoryColor, color: '#fff' }}
      />
    );
  };

  const priceBodyTemplate = (rowData) => {
    return `$${rowData.price}`;
  };

  const buyersBodyTemplate = (rowData) => {
    const buyersCount = rowData.BuyersCount || rowData.buyersCount || 0;
    return (
      <span className="p-badge p-badge-info">
        {buyersCount} buyers
      </span>
    );
  };

  const statusBodyTemplate = (rowData) => {
    if (rowData.isLotteryCompleted) {
      return (
        <Badge 
          value="Drawn" 
          severity="warning"
          icon="pi pi-trophy"
        />
      );
    }
    return (
      <Badge 
        value="Available" 
        severity="success"
        icon="pi pi-check"
      />
    );
  };

  const actionBodyTemplate = (rowData) => {
    return (
      <div className="flex gap-2">
        <Button
          icon="pi pi-pencil"
          className="p-button-rounded p-button-info p-button-sm"
          onClick={() => openEditDialog(rowData)}
          tooltip="Edit"
        />
        <Button
          icon="pi pi-trash"
          className="p-button-rounded p-button-danger p-button-sm"
          onClick={() => deleteGift(rowData)}
          tooltip="Delete"
          disabled={(rowData.BuyersCount || rowData.buyersCount || 0) > 0}
        />
      </div>
    );
  };

  if (!user || !isManager()) {
    return (
      <div className="flex justify-content-center align-items-center p-4">
        <Card title="Access Restricted">
          <p>Only managers can access gift management.</p>
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
          <h2 className="m-0">Gift Management</h2>
          <Button
            label="New Gift"
            icon="pi pi-plus"
            className="p-button-success"
            onClick={openNewDialog}
          />
        </div>

        <DataTable 
          value={gifts} 
          loading={loading}
          paginator
          rows={10}
          responsiveLayout="scroll"
          emptyMessage="No gifts found"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
          currentPageReportTemplate="Showing {first} to {last} of {totalRecords} gifts"
        >
          <Column field="name" header="Name" sortable />
          <Column header="Category" body={categoryBodyTemplate} />
          <Column field="donorName" header="Donor" sortable />
          <Column header="Ticket Price" body={priceBodyTemplate} sortable />
          <Column header="Purchases" body={buyersBodyTemplate} />
          <Column header="Status" body={statusBodyTemplate} />
          <Column header="Actions" body={actionBodyTemplate} style={{ width: '120px' }} />
        </DataTable>
      </Card>

      <Dialog
        visible={showDialog}
        style={{ width: '500px' }}
        header={isEditing ? "Edit Gift" : "New Gift"}
        modal
        onHide={hideDialog}
      >
        <div className="p-4">
          <div className="field mb-3">
            <label htmlFor="name" className="block mb-2">Gift Name *</label>
            <InputText
              id="name"
              value={giftForm.name}
              onChange={(e) => setGiftForm({...giftForm, name: e.target.value})}
              className="w-full"
              placeholder="Enter gift name"
            />
          </div>

          <div className="field mb-3">
            <label htmlFor="category" className="block mb-2">Category *</label>
            <Dropdown
              id="category"
              value={giftForm.categoryId}
              options={categories}
              onChange={(e) => setGiftForm({...giftForm, categoryId: e.value})}
              optionLabel="name"
              optionValue="id"
              placeholder="Select category"
              className="w-full"
            />
          </div>

          <div className="field mb-3">
            <label htmlFor="donor" className="block mb-2">Donor *</label>
            <Dropdown
              id="donor"
              value={giftForm.donorId}
              options={donors}
              onChange={(e) => setGiftForm({...giftForm, donorId: e.value})}
              optionLabel="name"
              optionValue="id"
              placeholder="Select donor"
              className="w-full"
            />
          </div>

          <div className="field mb-3">
            <label htmlFor="price" className="block mb-2">Ticket Price *</label>
            <InputNumber
              id="price"
              value={giftForm.price}
              onValueChange={(e) => setGiftForm({...giftForm, price: e.value})}
              mode="currency"
              currency="ILS"
              locale="en-US"
              className="w-full"
              min={0}
            />
          </div>

          <div className="flex justify-content-end gap-2">
            <Button
              label="Cancel"
              icon="pi pi-times"
              className="p-button-text"
              onClick={hideDialog}
            />
            <Button
              label={isEditing ? "Update" : "Create"}
              icon="pi pi-check"
              onClick={saveGift}
            />
          </div>
        </div>
      </Dialog>
    </div>
  );
}
