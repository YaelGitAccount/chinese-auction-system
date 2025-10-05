import api from "../api/axios.js";

// פונקציית עזר לבדיקת חיבור לשרת
export const checkServerConnection = async () => {
  try {
    // נסיון לקרוא לendpoint פשוט
    const response = await fetch("http://localhost:5234/api", {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    });
    
    console.log("Server connection test:", {
      status: response.status,
      ok: response.ok,
      url: response.url
    });
    
    return response.ok;
  } catch (error) {
    console.error("Server connection failed:", error);
    return false;
  }
};

// פונקציית עזר לבדיקת endpoints זמינים
export const checkAvailableEndpoints = async () => {
  const endpoints = [
    { path: "/Gift", method: "GET", description: "Get all gifts" },
    { path: "/Purchase", method: "GET", description: "Get all purchases" },
    { path: "/Purchase", method: "POST", description: "Create purchase" },
    { path: "/Purchase/Add", method: "POST", description: "Add to cart" },
    { path: "/Purchase/AddToCart", method: "POST", description: "Add to cart (alt)" },
    { path: "/Cart", method: "GET", description: "Get cart" },
    { path: "/Cart", method: "POST", description: "Add to cart" },
    { path: "/User", method: "GET", description: "Get users" },
    { path: "/Donor", method: "GET", description: "Get donors" }
  ];
  
  const results = {};
  
  for (const endpoint of endpoints) {
    try {
      let response;
      
      if (endpoint.method === "GET") {
        response = await api.get(endpoint.path);
        results[`${endpoint.method} ${endpoint.path}`] = {
          status: "success",
          statusCode: response.status,
          dataType: Array.isArray(response.data) ? `array[${response.data.length}]` : typeof response.data,
          description: endpoint.description
        };
      } else if (endpoint.method === "POST") {
        // לא ננסה POST כי זה יכול לשבש את הדאטה
        results[`${endpoint.method} ${endpoint.path}`] = {
          status: "skipped",
          reason: "POST requests skipped to avoid data corruption",
          description: endpoint.description
        };
      }
      
    } catch (error) {
      results[`${endpoint.method} ${endpoint.path}`] = {
        status: "error",
        statusCode: error.response?.status || "no response",
        error: error.message,
        description: endpoint.description
      };
    }
  }
  
  console.log("=== Available Endpoints Report ===");
  console.table(results);
  return results;
};

export default { checkServerConnection, checkAvailableEndpoints };
