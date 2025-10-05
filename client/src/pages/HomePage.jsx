import React from "react";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

export default function HomePage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  // If user is not logged in - show login/register options
  if (!user) {
    return (
      <div className="flex justify-content-center align-items-center min-h-screen">
        <Card 
          title={
            <div className="text-center">
              <div className="text-6xl mb-3">🎁</div>
              <h2 className="text-3xl font-bold m-0">Welcome to Chinese Auction</h2>
            </div>
          } 
          className="w-full max-w-md shadow-8"
          style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', color: 'white' }}
        >
          <div className="text-center mb-4">
            <p className="text-xl">Join our exciting auction and win amazing prizes!</p>
          </div>
          <div className="flex flex-column gap-3">
            <Button 
              label="Login" 
              icon="pi pi-user" 
              onClick={() => navigate("/login")}
              className="w-full p-button-outlined"
              style={{ borderColor: 'white', color: 'white' }}
            />
            <Button 
              label="Register" 
              icon="pi pi-user-plus" 
              onClick={() => navigate("/register")}
              className="w-full"
              style={{ background: 'white', color: '#667eea', border: 'none' }}
            />
            <Button 
              label="View Available Gifts" 
              icon="pi pi-gift" 
              onClick={() => navigate("/gifts")}
              className="w-full p-button-text"
              style={{ color: 'white' }}
            />
          </div>
        </Card>
      </div>
    );
  }

  // If user is logged in - show navigation based on permissions
  const isManager = user["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] === "manager";
  
  // Get name from JWT token
  const userName = user["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "User";

  return (
    <div className="p-4">
      <div className="mb-6 text-center">
        <h1 className="text-4xl font-bold m-0">Hello {userName}! 👋</h1>
        <p className="text-xl text-600 mt-2">Welcome to the Chinese Auction</p>
      </div>

      <div className="grid">
        {/* Options for regular user */}
        <div className="col-12 md:col-6">
          <Card 
            title="🛍️ Customer Actions" 
            className="h-full stats-card"
          >
            <div className="flex flex-column gap-3">
              <Button 
                label="Browse Gifts" 
                icon="pi pi-gift" 
                onClick={() => navigate("/gifts")}
                className="w-full p-button-lg"
              />
              <Button 
                label="My Shopping Cart" 
                icon="pi pi-shopping-cart" 
                onClick={() => navigate("/cart")}
                className="w-full p-button-lg p-button-outlined"
              />
              <Button 
                label="My Purchases" 
                icon="pi pi-list" 
                onClick={() => navigate("/my-purchases")}
                className="w-full p-button-lg p-button-outlined"
              />
              <Button 
                label="Lottery Results" 
                icon="pi pi-trophy" 
                onClick={() => navigate("/lottery-results")}
                className="w-full p-button-lg p-button-outlined"
              />
            </div>
          </Card>
        </div>

        {/* Options for manager */}
        {isManager && (
          <div className="col-12 md:col-6">
            <Card 
              title="⚙️ Management Panel" 
              className="h-full stats-card"
            >
              <div className="flex flex-column gap-3">
                <Button 
                  label="Manage Donors" 
                  icon="pi pi-users" 
                  onClick={() => navigate("/admin/donors-management")}
                  className="w-full p-button-lg"
                />
                <Button 
                  label="Manage Gifts" 
                  icon="pi pi-gift" 
                  onClick={() => navigate("/admin/gifts")}
                  className="w-full p-button-lg"
                />
                <Button 
                  label="Manage Categories" 
                  icon="pi pi-tags" 
                  onClick={() => navigate("/admin/categories")}
                  className="w-full p-button-lg"
                />
                <Button 
                  label="Lottery Management" 
                  icon="pi pi-star" 
                  onClick={() => navigate("/admin/lottery")}
                  className="w-full p-button-lg p-button-warning"
                />
                <Button 
                  label="Reports & Statistics" 
                  icon="pi pi-chart-bar" 
                  onClick={() => navigate("/admin/reports")}
                  className="w-full p-button-lg p-button-info"
                />
              </div>
            </Card>
          </div>
        )}
      </div>
    </div>
  );
}
