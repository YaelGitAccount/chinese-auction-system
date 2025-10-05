import React, { useState } from "react";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import { useNavigate, Link } from "react-router-dom";
import api from "../api/axios.js";
import { useAuth } from "../context/AuthContext.jsx";

export default function RegisterPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    name: "",
    email: "",
    phone: "",
    password: "",
    confirmPassword: ""
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    if (form.password !== form.confirmPassword) {
      setError("Passwords do not match");
      setLoading(false);
      return;
    }

    try {
      const { data } = await api.post("/User/register", {
        name: form.name,
        email: form.email,
        phone: form.phone,
        password: form.password
      });
      
      login(data.token);
      navigate("/");
    } catch (err) {
      setError(err.response?.data?.message || "Registration error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex justify-content-center align-items-center p-4">
      <Card title="New Customer Registration" className="w-full max-w-md">
        <form onSubmit={handleSubmit} className="p-fluid">
          <div className="field">
            <label htmlFor="name">Full Name</label>
            <InputText
              id="name"
              name="name"
              value={form.name}
              onChange={handleChange}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="email">Email</label>
            <InputText
              id="email"
              name="email"
              type="email"
              value={form.email}
              onChange={handleChange}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="phone">Phone</label>
            <InputText
              id="phone"
              name="phone"
              value={form.phone}
              onChange={handleChange}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="password">Password</label>
            <Password
              id="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              required
              toggleMask
            />
          </div>

          <div className="field">
            <label htmlFor="confirmPassword">Confirm Password</label>
            <Password
              id="confirmPassword"
              name="confirmPassword"
              value={form.confirmPassword}
              onChange={handleChange}
              required
              toggleMask
            />
          </div>

          {error && <Message severity="error" text={error} className="mb-3" />}

          <Button
            type="submit"
            label="Register"
            icon="pi pi-user-plus"
            loading={loading}
            className="mb-3"
          />

          <div className="text-center">
            <span>Already have an account? </span>
            <Link to="/login" className="text-primary">Login here</Link>
          </div>
        </form>
      </Card>
    </div>
  );
}
