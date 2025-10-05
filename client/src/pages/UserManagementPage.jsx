import React, { useState, useEffect, useRef } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Toast } from "primereact/toast";
import { Badge } from "primereact/badge";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { useAuth } from "../context/AuthContext.jsx";
import api from "../api/axios.js";

export default function UserManagementPage() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const { user, isManager } = useAuth();
  const toast = useRef(null);

  useEffect(() => {
    if (user && isManager()) {
      loadUsers();
    }
  }, [user]);

  const loadUsers = async () => {
    try {
      setLoading(true);
      const response = await api.get("/User");
      setUsers(response.data);
    } catch (error) {
      console.error("Error loading users:", error);
      showToast("error", "Error", "Unable to load users list");
    } finally {
      setLoading(false);
    }
  };

  const showToast = (severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, life: 3000 });
  };

  const handleDeleteUser = (userData) => {
    if (userData.role === "manager") {
      showToast("warn", "Warning", "Cannot delete manager users");
      return;
    }

    confirmDialog({
      message: `Are you sure you want to delete user "${userData.name}"?`,
      header: "Confirm Deletion",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          await api.delete(`/User/${userData.id}`);
          showToast("success", "Success", "User deleted successfully");
          loadUsers();
        } catch (error) {
          console.error("Error deleting user:", error);
          let errorMessage = "Error deleting user";
          if (error.response?.status === 400) {
            errorMessage = "Cannot delete user - may have existing data";
          }
          showToast("error", "Error", errorMessage);
        }
      },
      acceptLabel: "Yes, Delete",
      rejectLabel: "Cancel",
      acceptClassName: "p-button-danger"
    });
  };

  const roleBodyTemplate = (rowData) => {
    const severity = rowData.role === "manager" ? "danger" : "info";
    return (
      <Badge 
        value={rowData.role === "manager" ? "Manager" : "Customer"} 
        severity={severity}
      />
    );
  };

  const actionBodyTemplate = (rowData) => {
    return (
      <Button
        icon="pi pi-trash"
        className="p-button-rounded p-button-danger p-button-sm"
        onClick={() => handleDeleteUser(rowData)}
        disabled={rowData.role === "manager"}
        tooltip={rowData.role === "manager" ? "Cannot delete managers" : "Delete user"}
      />
    );
  };

  if (!user || !isManager()) {
    return (
      <div className="flex justify-content-center align-items-center p-4">
        <Card title="Access Restricted">
          <p>Only managers can access user management.</p>
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
          <h2 className="m-0">User Management</h2>
          <Badge 
            value={`${users.length} users`} 
            className="p-badge-lg" 
          />
        </div>

        {users.length === 0 && !loading ? (
          <div className="text-center p-4">
            <i className="pi pi-users" style={{ fontSize: "3rem", color: "#ccc" }}></i>
            <h3>No Users</h3>
            <p>No users found in the system.</p>
          </div>
        ) : (
          <DataTable 
            value={users} 
            loading={loading}
            paginator 
            rows={10}
            responsiveLayout="scroll"
            emptyMessage="No users found"
            sortField="name"
            sortOrder={1}
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} users"
          >
            <Column 
              field="name" 
              header="Name" 
              sortable
              style={{ fontWeight: 'bold' }}
            />
            <Column 
              field="email" 
              header="Email" 
              sortable 
            />
            <Column 
              field="phone" 
              header="Phone" 
              sortable 
            />
            <Column 
              field="role" 
              header="Role" 
              body={roleBodyTemplate}
              sortable 
            />
            <Column 
              header="Actions"
              body={actionBodyTemplate}
              style={{ width: '100px' }}
            />
          </DataTable>
        )}

        <div className="mt-4 text-center">
          <Button
            label="Refresh Data"
            icon="pi pi-refresh"
            className="p-button-outlined"
            onClick={loadUsers}
            loading={loading}
          />
        </div>
      </Card>
    </div>
  );
}
