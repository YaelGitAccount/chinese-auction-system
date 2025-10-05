import React, { useEffect, useState, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { Toast } from "primereact/toast";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import api from "../api/axios.js";

export default function DonorsManagementPage() {
  const [donors, setDonors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showDialog, setShowDialog] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [selectedDonor, setSelectedDonor] = useState(null);
  const [donorForm, setDonorForm] = useState({ name: "", email: "", phone: "" });
  const toast = useRef(null);

  useEffect(() => {
    loadDonors();
  }, []);

  const loadDonors = async () => {
    setLoading(true);
    try {
      const { data } = await api.get("/Donor");
      setDonors(data);
    } catch {
      showToast("error", "Error", "Unable to load donors list");
    }
    setLoading(false);
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const openNewDialog = () => {
    setDonorForm({ name: "", email: "", phone: "" });
    setIsEditing(false);
    setSelectedDonor(null);
    setShowDialog(true);
  };

  const openEditDialog = (donor) => {
    setDonorForm({ name: donor.name, email: donor.email, phone: donor.phone || "" });
    setIsEditing(true);
    setSelectedDonor(donor);
    setShowDialog(true);
  };

  const hideDialog = () => {
    setShowDialog(false);
    setDonorForm({ name: "", email: "", phone: "" });
    setSelectedDonor(null);
    setIsEditing(false);
  };

  const saveDonor = async () => {
    if (!donorForm.name.trim() || !donorForm.email.trim() || !donorForm.phone.trim()) {
      showToast("warn", "אזהרה", "יש למלא שם, אימייל וטלפון");
      return;
    }
    try {
      if (isEditing) {
        await api.put(`/Donor/${selectedDonor.id}`, donorForm);
        showToast("success", "הצלחה", "התורם עודכן בהצלחה");
      } else {
        await api.post("/Donor", donorForm);
        showToast("success", "הצלחה", "התורם נוסף בהצלחה");
      }
      hideDialog();
      loadDonors();
    } catch {
      showToast("error", "שגיאה", "שגיאה בשמירת התורם");
    }
  };

  const deleteDonor = (donor) => {
    confirmDialog({
      message: `האם למחוק את התורם "${donor.name}"?`,
      header: "אישור מחיקה",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          await api.delete(`/Donor/${donor.id}`);
          showToast("success", "הצלחה", "התורם נמחק");
          loadDonors();
        } catch {
          showToast("error", "שגיאה", "שגיאה במחיקת התורם");
        }
      },
      acceptLabel: "כן, מחק",
      rejectLabel: "ביטול",
      acceptClassName: "p-button-danger"
    });
  };

  const actionBodyTemplate = (rowData) => (
    <div className="flex gap-2">
      <Button icon="pi pi-pencil" className="p-button-rounded p-button-info p-button-sm" onClick={() => openEditDialog(rowData)} tooltip="ערוך" />
      <Button icon="pi pi-trash" className="p-button-rounded p-button-danger p-button-sm" onClick={() => deleteDonor(rowData)} tooltip="מחק" />
    </div>
  );

  return (
    <div className="p-4">
      <Toast ref={toast} />
      <ConfirmDialog />
      <Card>
        <div className="flex justify-content-between align-items-center mb-4">
          <h2 className="m-0">ניהול תורמים</h2>
          <Button label="תורם חדש" icon="pi pi-plus" className="p-button-success" onClick={openNewDialog} />
        </div>
        <DataTable value={donors} loading={loading} paginator rows={10} responsiveLayout="scroll" emptyMessage="לא נמצאו תורמים">
          <Column field="name" header="Name" sortable />
          <Column field="email" header="Email" sortable />
          <Column field="phone" header="Phone" sortable />
          <Column header="Actions" body={actionBodyTemplate} style={{ width: '120px' }} />
        </DataTable>
      </Card>
      <Dialog visible={showDialog} style={{ width: '400px' }} header={isEditing ? "עריכת תורם" : "תורם חדש"} modal onHide={hideDialog}>
        <div className="p-4">
          <div className="field mb-3">
            <label htmlFor="name" className="block mb-2">שם *</label>
            <InputText id="name" value={donorForm.name} onChange={e => setDonorForm({ ...donorForm, name: e.target.value })} className="w-full" placeholder="הכנס שם" />
          </div>
          <div className="field mb-3">
            <label htmlFor="email" className="block mb-2">אימייל *</label>
            <InputText id="email" value={donorForm.email} onChange={e => setDonorForm({ ...donorForm, email: e.target.value })} className="w-full" placeholder="הכנס אימייל" />
          </div>
          <div className="field mb-3">
            <label htmlFor="phone" className="block mb-2">טלפון *</label>
            <InputText id="phone" value={donorForm.phone} onChange={e => setDonorForm({ ...donorForm, phone: e.target.value })} className="w-full" placeholder="הכנס טלפון" />
          </div>
          <div className="flex justify-content-end gap-2">
            <Button label="ביטול" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label={isEditing ? "עדכן" : "צור"} icon="pi pi-check" onClick={saveDonor} />
          </div>
        </div>
      </Dialog>
    </div>
  );
}
