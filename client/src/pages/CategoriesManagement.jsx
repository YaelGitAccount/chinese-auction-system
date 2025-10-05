import React, { useState, useEffect, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { InputTextarea } from "primereact/inputtextarea";
import { ColorPicker } from "primereact/colorpicker";
import { Toast } from "primereact/toast";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { Checkbox } from "primereact/checkbox";
import { useAuth } from "../context/AuthContext.jsx";
import * as categoriesApi from "../api/categories.js";

export default function CategoriesManagement() {
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showDialog, setShowDialog] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [categoryForm, setCategoryForm] = useState({
    name: "",
    description: "",
    color: "#007bff",
    isActive: true
  });
  const { user, isManager } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    if (user && isManager()) {
      loadCategories();
    }
  }, [user]);

  const loadCategories = async () => {
    try {
      setLoading(true);
      const data = await categoriesApi.getAllCategories();
      setCategories(data);
    } catch (error) {
      console.error("Error loading categories:", error);
      showToast("error", "שגיאה", "לא ניתן לטעון את רשימת הקטגוריות");
    } finally {
      setLoading(false);
    }
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const openNewDialog = () => {
    setCategoryForm({
      name: "",
      description: "",
      color: "#007bff",
      isActive: true
    });
    setIsEditing(false);
    setSelectedCategory(null);
    setShowDialog(true);
  };

  const openEditDialog = (category) => {
    setCategoryForm({
      name: category.name,
      description: category.description || "",
      color: category.color || "#007bff",
      isActive: category.isActive
    });
    setIsEditing(true);
    setSelectedCategory(category);
    setShowDialog(true);
  };

  const hideDialog = () => {
    setShowDialog(false);
    setCategoryForm({
      name: "",
      description: "",
      color: "#007bff",
      isActive: true
    });
    setSelectedCategory(null);
    setIsEditing(false);
  };

  const saveCategory = async () => {
    try {
      if (!categoryForm.name?.trim()) {
        showToast("warn", "אזהרה", "יש למלא שם קטגוריה");
        return;
      }

      if (isEditing) {
        await categoriesApi.updateCategory(selectedCategory.id, categoryForm);
        showToast("success", "הצלחה", "הקטגוריה עודכנה בהצלחה");
      } else {
        await categoriesApi.createCategory(categoryForm);
        showToast("success", "הצלחה", "הקטגוריה נוצרה בהצלחה");
      }

      hideDialog();
      loadCategories();
    } catch (error) {
      console.error("Error saving category:", error);
      let errorMessage = "שגיאה בשמירת הקטגוריה";
      if (error.response?.status === 400) {
        errorMessage = "קטגוריה עם השם הזה כבר קיימת או נתונים לא תקינים";
      }
      showToast("error", "שגיאה", errorMessage);
    }
  };

  const deleteCategory = async (category) => {
    confirmDialog({
      message: `האם אתה בטוח שברצונך למחוק את הקטגוריה "${category.name}"?`,
      header: "אישור מחיקה",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          await categoriesApi.deleteCategory(category.id);
          showToast("success", "הצלחה", "הקטגוריה נמחקה בהצלחה");
          loadCategories();
        } catch (error) {
          console.error("Error deleting category:", error);
          let errorMessage = "שגיאה במחיקת הקטגוריה";
          if (error.response?.status === 400) {
            errorMessage = "לא ניתן למחוק קטגוריה שיש בה מתנות";
          }
          showToast("error", "שגיאה", errorMessage);
        }
      },
      acceptLabel: "כן, מחק",
      rejectLabel: "ביטול",
      acceptClassName: "p-button-danger"
    });
  };

  const colorBodyTemplate = (rowData) => {
    return (
      <div className="flex align-items-center gap-2">
        <div 
          style={{ 
            width: "20px", 
            height: "20px", 
            backgroundColor: rowData.color || "#6c757d",
            borderRadius: "3px",
            border: "1px solid #ccc"
          }}
        ></div>
        <span>{rowData.color || "#6c757d"}</span>
      </div>
    );
  };

  const statusBodyTemplate = (rowData) => {
    return (
      <span className={`p-badge ${rowData.isActive ? 'p-badge-success' : 'p-badge-secondary'}`}>
        {rowData.isActive ? "פעיל" : "לא פעיל"}
      </span>
    );
  };

  const giftsCountBodyTemplate = (rowData) => {
    return (
      <span className="p-badge p-badge-info">
        {rowData.giftsCount || 0} מתנות
      </span>
    );
  };

  const actionBodyTemplate = (rowData) => {
    return (
      <div className="flex gap-2">
        <Button
          icon="pi pi-pencil"
          className="p-button-rounded p-button-info p-button-sm"
          onClick={() => openEditDialog(rowData)}
          tooltip="ערוך"
        />
        <Button
          icon="pi pi-trash"
          className="p-button-rounded p-button-danger p-button-sm"
          onClick={() => deleteCategory(rowData)}
          tooltip="מחק"
          disabled={rowData.giftsCount > 0}
        />
      </div>
    );
  };

  if (!user || !isManager()) {
    return (
      <div className="flex justify-content-center align-items-center p-4">
        <Card title="Access Restricted">
          <p>Only managers can access category management.</p>
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
          <h2 className="m-0">ניהול קטגוריות</h2>
          <Button
            label="קטגוריה חדשה"
            icon="pi pi-plus"
            className="p-button-success"
            onClick={openNewDialog}
          />
        </div>

        <DataTable 
          value={categories} 
          loading={loading}
          responsiveLayout="scroll"
          emptyMessage="לא נמצאו קטגוריות"
        >
          <Column field="name" header="שם" sortable />
          <Column field="description" header="תיאור" />
          <Column header="צבע" body={colorBodyTemplate} />
          <Column header="סטטוס" body={statusBodyTemplate} />
          <Column header="מספר מתנות" body={giftsCountBodyTemplate} />
          <Column header="פעולות" body={actionBodyTemplate} style={{ width: '120px' }} />
        </DataTable>
      </Card>

      <Dialog
        visible={showDialog}
        style={{ width: '450px' }}
        header={isEditing ? "עריכת קטגוריה" : "קטגוריה חדשה"}
        modal
        onHide={hideDialog}
      >
        <div className="p-4">
          <div className="field mb-3">
            <label htmlFor="name" className="block mb-2">שם הקטגוריה *</label>
            <InputText
              id="name"
              value={categoryForm.name}
              onChange={(e) => setCategoryForm({...categoryForm, name: e.target.value})}
              className="w-full"
              placeholder="הכנס שם קטגוריה"
            />
          </div>

          <div className="field mb-3">
            <label htmlFor="description" className="block mb-2">תיאור</label>
            <InputTextarea
              id="description"
              value={categoryForm.description}
              onChange={(e) => setCategoryForm({...categoryForm, description: e.target.value})}
              className="w-full"
              rows={3}
              placeholder="תיאור הקטגוריה (אופציונלי)"
            />
          </div>

          <div className="field mb-3">
            <label htmlFor="color" className="block mb-2">צבע</label>
            <div className="flex align-items-center gap-2">
              <ColorPicker
                value={categoryForm.color.replace('#', '')}
                onChange={(e) => setCategoryForm({...categoryForm, color: `#${e.value}`})}
              />
              <InputText
                value={categoryForm.color}
                onChange={(e) => setCategoryForm({...categoryForm, color: e.target.value})}
                placeholder="#007bff"
                className="w-full"
              />
            </div>
          </div>

          <div className="field mb-3">
            <div className="flex align-items-center">
              <Checkbox
                inputId="isActive"
                checked={categoryForm.isActive}
                onChange={(e) => setCategoryForm({...categoryForm, isActive: e.checked})}
              />
              <label htmlFor="isActive" className="ml-2">קטגוריה פעילה</label>
            </div>
          </div>

          <div className="flex justify-content-end gap-2">
            <Button
              label="ביטול"
              icon="pi pi-times"
              className="p-button-text"
              onClick={hideDialog}
            />
            <Button
              label={isEditing ? "עדכן" : "צור"}
              icon="pi pi-check"
              onClick={saveCategory}
            />
          </div>
        </div>
      </Dialog>
    </div>
  );
}
