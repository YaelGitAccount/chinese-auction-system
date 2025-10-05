import { useEffect, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import api from "../../api/axios.js";

export default function DonorsList() {
  const [donors, setDonors] = useState([]);
  useEffect(() => { api.get("/Donor").then(res => setDonors(res.data)); }, []);
  return (
    <div className="p-4">
      <h2>רשימת תורמים</h2>
      <DataTable value={donors} paginator rows={10} responsiveLayout="scroll">
        <Column field="name" header="שם" />
        <Column field="email" header="אימייל" />
        <Column field="phone" header="טלפון" />
        <Column field="gifts" header="מתנות" body={row => row.gifts?.map(g => g.name).join(", ")} />
        <Column body={row => <Button icon="pi pi-pencil" className="p-button-text" />} />
      </DataTable>
    </div>
  );
}
