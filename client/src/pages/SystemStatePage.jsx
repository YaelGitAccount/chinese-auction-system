import React, { useEffect, useState } from "react";
import api from "../api/axios";

export default function SystemStatePage() {
  const [state, setState] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  useEffect(() => {
    fetchState();
  }, []);

  const fetchState = async () => {
    setLoading(true);
    try {
      const { data } = await api.get("/SystemState");
      setState(data);
    } catch (e) {
      setError("שגיאה בטעינת מצב מערכת");
    }
    setLoading(false);
  };

  const handleAction = async (action) => {
    setMessage("");
    try {
      await api.post(`/SystemState/${action}`);
      setMessage("בוצע בהצלחה");
      fetchState();
    } catch {
      setError("שגיאה בביצוע פעולה");
    }
  };

  return (
    <div style={{ maxWidth: 700, margin: "40px auto", padding: 24, background: "#fff", borderRadius: 12, boxShadow: "0 2px 8px #eee" }}>
      <h2 style={{ textAlign: "center", marginBottom: 24 }}>ניהול מצב מערכת</h2>
      {error && <div style={{ color: "#c00", marginBottom: 16 }}>{error}</div>}
      {message && <div style={{ color: "#080", marginBottom: 16 }}>{message}</div>}
      {loading ? (
        <div>טוען...</div>
      ) : state ? (
        <>
          <div style={{ marginBottom: 24 }}>
            <b>סטטוס נוכחי:</b> {state.status}
          </div>
          <button onClick={() => handleAction("startLottery")}>התחל הגרלה</button>
          <button onClick={() => handleAction("endLottery")} style={{ marginRight: 8 }}>סיים הגרלה</button>
          <button onClick={() => handleAction("resetSystem")} style={{ marginRight: 8 }}>אפס מערכת</button>
        </>
      ) : null}
    </div>
  );
}
