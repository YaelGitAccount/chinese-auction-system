import { useState } from "react";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import { Link, useNavigate } from "react-router-dom";
import api from "../../api/axios.js";
import { useAuth } from "../../context/AuthContext.jsx";

export default function LoginForm() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = e => setForm({ ...form, [e.target.name]: e.target.value });
  
  const handleSubmit = async e => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const { data } = await api.post("/User/login", form);
      login(data.token);
      navigate("/");
    } catch (err) {
      setError(err.response?.data?.message || "Login error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex justify-content-center align-items-center p-4">
      <Card title="Login" className="w-full max-w-md">
        <form onSubmit={handleSubmit} className="p-fluid">
          <div className="field">
            <label htmlFor="email">Email</label>
            <InputText 
              id="email"
              name="email" 
              value={form.email} 
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

          {error && <Message severity="error" text={error} className="mb-3" />}

          <Button 
            type="submit"
            label="Login" 
            icon="pi pi-sign-in" 
            loading={loading}
            className="mb-3"
          />

          <div className="text-center">
            <span>Don't have an account yet? </span>
            <Link to="/register" className="text-primary">Register here</Link>
          </div>
        </form>
      </Card>
    </div>
  );
}
