import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "./context/AuthContext.jsx";
import Navbar from "./components/shared/Navbar.jsx";
import HomePage from "./pages/HomePage.jsx";
import RegisterPage from "./pages/RegisterPage.jsx";
import CartPage from "./pages/CartPage.jsx";
import MyPurchasesPage from "./pages/MyPurchasesPage.jsx";
import PurchasesManagementPage from "./pages/PurchasesManagementPage.jsx";
import LoginForm from "./components/auth/LoginForm.jsx";
import DonorsList from "./components/admin/DonorsList.jsx";
import GiftsList from "./components/shared/GiftsList.jsx";
import IncomeReport from "./components/admin/IncomeReport.jsx";
import CategoriesManagement from "./pages/CategoriesManagement.jsx";
import DonorsPage from "./pages/DonorsPage.jsx";
import UserManagementPage from "./pages/UserManagementPage.jsx";
import SystemStatePage from "./pages/SystemStatePage.jsx";
import StatisticsPage from "./pages/StatisticsPage.jsx";
import LotteryPage from "./pages/LotteryPage.jsx";
import LotteryManagementPage from "./pages/LotteryManagementPage.jsx";
import LotteryResultsPage from "./pages/LotteryResultsPage.jsx";
import GiftsManagement from "./pages/GiftsManagement.jsx";
import DonorsManagementPage from "./pages/DonorsManagementPage.jsx";

// API base configuration for the server in the parent directory (localhost:5234)
// All API calls in api/axios.js already point to http://localhost:5234/api
// No need to change anything here, just ensure the server is running in ../server

function PrivateRoute({ children, role }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" />;
  if (role && user["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] !== role)
    return <Navigate to="/login" />;
  return children;
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <div className="min-h-screen" style={{ backgroundColor: '#f8f9fa' }}>
          <Navbar />
          <main className="page-container">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/login" element={<LoginForm />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/gifts" element={<GiftsList />} />
              <Route path="/lottery-results" element={<LotteryResultsPage />} />
              <Route path="/cart" element={<CartPage />} />
              <Route path="/my-purchases" element={<MyPurchasesPage />} />
              <Route path="/admin/donors" element={<PrivateRoute role="manager"><DonorsList /></PrivateRoute>} />
              <Route path="/admin/categories" element={<PrivateRoute role="manager"><CategoriesManagement /></PrivateRoute>} />
              <Route path="/admin/income" element={<PrivateRoute role="manager"><IncomeReport /></PrivateRoute>} />
              <Route path="/admin/gifts" element={<PrivateRoute role="manager"><GiftsManagement /></PrivateRoute>} />
              <Route path="/admin/purchases" element={<PrivateRoute role="manager"><PurchasesManagementPage /></PrivateRoute>} />
              <Route path="/admin/lottery" element={<PrivateRoute role="manager"><LotteryManagementPage /></PrivateRoute>} />
              <Route path="/admin/lottery-results" element={<PrivateRoute role="manager"><LotteryResultsPage /></PrivateRoute>} />
              <Route path="/admin/reports" element={<PrivateRoute role="manager"><StatisticsPage /></PrivateRoute>} />
              <Route path="/admin/users" element={<PrivateRoute role="manager"><UserManagementPage /></PrivateRoute>} />
              <Route path="/admin/system" element={<PrivateRoute role="manager"><SystemStatePage /></PrivateRoute>} />
              <Route path="/admin/donors-management" element={<PrivateRoute role="manager"><DonorsManagementPage /></PrivateRoute>} />
              <Route path="/donors" element={<DonorsPage />} />
              <Route path="*" element={<Navigate to="/" />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </AuthProvider>
  );
}
