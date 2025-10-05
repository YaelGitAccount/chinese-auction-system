import React, { useEffect, useState } from "react";
import { getAllLotteries, createLottery, runLottery } from "../api/lottery";

export default function LotteryPage() {
  const [lotteries, setLotteries] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ name: "", date: "" });

  useEffect(() => {
    fetchLotteries();
  }, []);

  const fetchLotteries = async () => {
    setLoading(true);
    try {
      const data = await getAllLotteries();
      setLotteries(data);
    } catch (e) {
      setError("שגיאה בטעינת הגרלות");
    }
    setLoading(false);
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      await createLottery(form);
      setShowCreate(false);
      setForm({ name: "", date: "" });
      fetchLotteries();
    } catch {
      setError("שגיאה ביצירת הגרלה");
    }
  };

  const handleRun = async (id) => {
    try {
      await runLottery(id);
      fetchLotteries();
    } catch {
      setError("שגיאה בהרצת הגרלה");
    }
  };

  return (
    <div style={{ maxWidth: 700, margin: "40px auto", padding: 24, background: "#fff", borderRadius: 12, boxShadow: "0 2px 8px #eee" }}>
      <h2 style={{ textAlign: "center", marginBottom: 24 }}>ניהול הגרלות</h2>
      {error && <div style={{ color: "#c00", marginBottom: 16 }}>{error}</div>}
      {loading ? (
        <div>טוען...</div>
      ) : (
        <>
          <button style={{ float: "left", marginBottom: 16 }} onClick={() => setShowCreate(!showCreate)}>
            {showCreate ? "ביטול" : "הוסף הגרלה"}
          </button>
          {showCreate && (
            <form onSubmit={handleCreate} style={{ marginBottom: 24 }}>
              <input
                required
                placeholder="שם הגרלה"
                value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                style={{ marginRight: 8 }}
              />
              <input
                required
                type="date"
                value={form.date}
                onChange={e => setForm(f => ({ ...f, date: e.target.value }))}
                style={{ marginRight: 8 }}
              />
              <button type="submit">צור</button>
            </form>
          )}
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr style={{ background: "#f5f5f5" }}>
                <th style={{ padding: 8, border: "1px solid #eee" }}>שם</th>
                <th style={{ padding: 8, border: "1px solid #eee" }}>תאריך</th>
                <th style={{ padding: 8, border: "1px solid #eee" }}>סטטוס</th>
                <th style={{ padding: 8, border: "1px solid #eee" }}>פעולות</th>
              </tr>
            </thead>
            <tbody>
              {lotteries.map(lottery => (
                <tr key={lottery.id}>
                  <td style={{ padding: 8, border: "1px solid #eee" }}>{lottery.name}</td>
                  <td style={{ padding: 8, border: "1px solid #eee" }}>{lottery.date}</td>
                  <td style={{ padding: 8, border: "1px solid #eee" }}>{lottery.status}</td>
                  <td style={{ padding: 8, border: "1px solid #eee" }}>
                    <button onClick={() => handleRun(lottery.id)} disabled={lottery.status === "הושלם"}>
                      הרץ הגרלה
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
}
