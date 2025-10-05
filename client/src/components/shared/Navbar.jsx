import React from "react";
import { Menubar } from "primereact/menubar";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import { Avatar } from "primereact/avatar";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext.jsx";

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  // Navigation function
  const navigateTo = (path) => {
    navigate(path);
  };

  // Enhanced brand logo component
  const brandLogo = (
    <div className="navbar-brand">
      <div className="brand-icon">
        <i className="pi pi-gift"></i>
      </div>
      <div className="brand-text">
        <span className="brand-title">Chinese Auction</span>
        <span className="brand-subtitle">Elegant Bidding Experience</span>
      </div>
    </div>
  );

  // If user is not logged in
  if (!user) {
    const items = [
      {
        label: "Home",
        icon: "pi pi-home",
        command: () => navigateTo("/"),
        className: "nav-item"
      },
      {
        label: "Gifts",
        icon: "pi pi-gift",
        command: () => navigateTo("/gifts"),
        className: "nav-item"
      },
      {
        label: "Lottery Results",
        icon: "pi pi-trophy",
        command: () => navigateTo("/lottery-results"),
        className: "nav-item"
      },
      {
        separator: true
      },
      {
        label: "Login",
        icon: "pi pi-sign-in",
        command: () => navigateTo("/login"),
        className: "nav-item nav-item-accent"
      },
      {
        label: "Register",
        icon: "pi pi-user-plus",
        command: () => navigateTo("/register"),
        className: "nav-item nav-item-primary"
      }
    ];

    return (
      <div className="navbar-container">
        <Menubar 
          model={items} 
          start={brandLogo}
          className="modern-navbar"
        />
      </div>
    );
  }

  // If user is logged in
  const { isManager } = useAuth();
  
  // Get name from JWT token
  const userName = user["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "User";

  const items = [
    {
      label: "Home",
      icon: "pi pi-home",
      command: () => navigateTo("/"),
      className: "nav-item"
    },
    {
      label: "Gifts",
      icon: "pi pi-gift",
      command: () => navigateTo("/gifts"),
      className: "nav-item"
    },
    {
      label: "Lottery Results",
      icon: "pi pi-trophy",
      command: () => navigateTo("/lottery-results"),
      className: "nav-item"
    },
    {
      label: "My Cart",
      icon: "pi pi-shopping-cart",
      command: () => navigateTo("/cart"),
      className: "nav-item"
    },
    {
      label: "My Purchases",
      icon: "pi pi-list",
      command: () => navigateTo("/my-purchases"),
      className: "nav-item"
    }
  ];

  // Add management items for managers
  if (isManager()) {
    items.push({
      label: "Management",
      icon: "pi pi-cog",
      className: "nav-item nav-item-admin",
      items: [
        {
          label: "Donors",
          icon: "pi pi-users",
          command: () => navigateTo("/admin/donors-management"),
          className: "admin-submenu-item"
        },
        {
          label: "Categories",
          icon: "pi pi-tags",
          command: () => navigateTo("/admin/categories"),
          className: "admin-submenu-item"
        },
        {
          label: "Gifts",
          icon: "pi pi-gift",
          command: () => navigateTo("/admin/gifts"),
          className: "admin-submenu-item"
        },
        {
          label: "Purchases",
          icon: "pi pi-shopping-cart",
          command: () => navigateTo("/admin/purchases"),
          className: "admin-submenu-item"
        },
        {
          label: "Lottery",
          icon: "pi pi-star",
          command: () => navigateTo("/admin/lottery"),
          className: "admin-submenu-item"
        },
        {
          label: "Reports",
          icon: "pi pi-chart-bar",
          command: () => navigateTo("/admin/reports"),
          className: "admin-submenu-item"
        },
        {
          label: "Users",
          icon: "pi pi-user",
          command: () => navigateTo("/admin/users"),
          className: "admin-submenu-item"
        }
      ]
    });
  }

  const userProfile = (
    <div className="user-profile-section">
      {isManager() && (
        <Badge 
          value="Admin" 
          severity="warning" 
          className="admin-badge"
        />
      )}
      <div className="user-info">
        <Avatar 
          label={userName.charAt(0).toUpperCase()} 
          className="user-avatar"
          shape="circle"
          size="normal"
        />
        <div className="user-details">
          <span className="user-name">Hello, {userName}</span>
          {isManager() && <span className="user-role">Administrator</span>}
        </div>
      </div>
      <Button 
        icon="pi pi-sign-out" 
        onClick={logout}
        className="logout-button"
        tooltip="Logout"
        tooltipOptions={{ position: 'bottom' }}
        text
        rounded
      />
    </div>
  );

  return (
    <div className="navbar-container">
      <Menubar 
        model={items} 
        start={brandLogo}
        end={userProfile}
        className="modern-navbar"
      />
    </div>
  );
}
