import axios from "axios";

const api = axios.create({ 
  baseURL: "http://localhost:5234/api",
  headers: {
    'Content-Type': 'application/json',
  }
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  response => response,
  error => {
    // Handle errors here if needed
    return Promise.reject(error);
  }
);

export default api;
