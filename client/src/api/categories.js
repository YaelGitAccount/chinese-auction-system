import api from "./axios.js";

// קטגוריות - כל המשתמשים יכולים לראות קטגוריות פעילות
export const getActiveCategories = async () => {
  const response = await api.get("/Category");
  return response.data;
};

// רק למנהלים - כל הקטגוריות כולל לא פעילות
export const getAllCategories = async () => {
  const response = await api.get("/Category/all");
  return response.data;
};

// קבלת קטגוריה לפי ID
export const getCategoryById = async (id) => {
  const response = await api.get(`/Category/${id}`);
  return response.data;
};

// יצירת קטגוריה חדשה - רק למנהלים
export const createCategory = async (categoryData) => {
  const response = await api.post("/Category", categoryData);
  return response.data;
};

// עדכון קטגוריה - רק למנהלים
export const updateCategory = async (id, categoryData) => {
  const response = await api.put(`/Category/${id}`, categoryData);
  return response.data;
};

// מחיקת קטגוריה - רק למנהלים
export const deleteCategory = async (id) => {
  const response = await api.delete(`/Category/${id}`);
  return response.data;
};
