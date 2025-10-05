import React, { useEffect, useState } from "react";
import api from "../api/axios";

export default function DonorsPage() {
  const [donors, setDonors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchDonors();
  }, []);

  const fetchDonors = async () => {
    setLoading(true);
    try {
      const { data } = await api.get("/Donor");
      setDonors(data);
    } catch (e) {
      setError("שגיאה בטעינת תורמים");
    }
    setLoading(false);
  };

  return (
    <div style={{ maxWidth: 700, margin: "40px auto", padding: 24, background: "#fff", borderRadius: 12, boxShadow: "0 2px 8px #eee" }}>
      <h2 style={{ textAlign: "center", marginBottom: 24 }}>רשימת תורמים</h2>
      {error && <div style={{ color: "#c00", marginBottom: 16 }}>{error}</div>}
      {loading ? (
        <div>טוען...</div>
      ) : (
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead>
            <tr style={{ background: "#f5f5f5" }}>
              <th style={{ padding: 8, border: "1px solid #eee" }}>שם</th>
              <th style={{ padding: 8, border: "1px solid #eee" }}>אימייל</th>
              <th style={{ padding: 8, border: "1px solid #eee" }}>סכום תרומה</th>
            </tr>
          </thead>
          <tbody>
            {donors.map(donor => (
              <tr key={donor.id}>
                <td style={{ padding: 8, border: "1px solid #eee" }}>{donor.name}</td>
                <td style={{ padding: 8, border: "1px solid #eee" }}>{donor.email}</td>
                <td style={{ padding: 8, border: "1px solid #eee" }}>{donor.totalDonated}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
