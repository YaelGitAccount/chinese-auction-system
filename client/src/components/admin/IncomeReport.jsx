import { useState } from "react";
import { Button } from "primereact/button";
import api from "../../api/axios.js";

export default function IncomeReport() {
  const [report, setReport] = useState(null);
  const getReport = async (excel = false) => {
    if (excel) {
      const res = await api.get("/Purchase/summary/overall?export=excel", { responseType: "blob" });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", "overall-summary.xlsx");
      document.body.appendChild(link);
      link.click();
      link.remove();
    } else {
      const { data } = await api.get("/Purchase/summary/overall");
      setReport(data);
    }
  };
  return (
    <div className="p-4">
      <Button label="הצג דוח" onClick={() => getReport(false)} className="mr-2" />
      <Button label="הורד אקסל" onClick={() => getReport(true)} />
      {report && <pre>{JSON.stringify(report, null, 2)}</pre>}
    </div>
  );
}
