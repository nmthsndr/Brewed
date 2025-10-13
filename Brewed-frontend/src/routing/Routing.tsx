import { Routes, Route, Navigate } from "react-router-dom";
import useAuth from "../hooks/useAuth";
import BasicLayout from "../components/Layout/BasicLayout";
import Login from "../pages/Login";
import Register from "../pages/Register";
import ForgotPassword from "../pages/ForgotPassword";
import ConfirmEmail from "../pages/ConfirmEmail";
import Dashboard from "../pages/Dashboard";
import Products from "../pages/Products";
import ProductDetail from "../pages/ProductDetail";
import Cart from "../pages/Cart";
import Checkout from "../pages/Checkout";
import Orders from "../pages/Orders";
import Profile from "../pages/Profile";
import Categories from "../pages/Categories";
import Coupons  from "../pages/Coupons";
import AdminDashboard from "../pages/AdminDashboard";
import Users from "../pages/Users";

const Routing = () => {
  const { isLoggedIn } = useAuth();

  return (
    <Routes>
      {/* Public Routes */}
      <Route path="/login" element={!isLoggedIn ? <Login /> : <Navigate to="/app/dashboard" />} />
      <Route path="/register" element={!isLoggedIn ? <Register /> : <Navigate to="/app/dashboard" />} />
      <Route path="/forgot-password" element={!isLoggedIn ? <ForgotPassword /> : <Navigate to="/app/dashboard" />} />
      <Route path="/confirm-email" element={<ConfirmEmail />} />

      {/* Protected Routes */}
      <Route path="/app" element={isLoggedIn ? <BasicLayout /> : <Navigate to="/login" />}>
        <Route path="dashboard" element={<Dashboard />} />
        <Route path="products" element={<Products />} />
        <Route path="products/:id" element={<ProductDetail />} />
        <Route path="cart" element={<Cart />} />
        <Route path="checkout" element={<Checkout />} />
        <Route path="orders" element={<Orders />} />
        <Route path="profile" element={<Profile />} />
        <Route path="categories" element={<Categories />} />
        <Route path="coupons" element={<Coupons />} />
        <Route path="admin-dashboard" element={<AdminDashboard />} />
      </Route>

      {/* Default Route */}
      <Route path="/" element={<Navigate to={isLoggedIn ? "/app/dashboard" : "/login"} />} />
      <Route path="*" element={<Navigate to={isLoggedIn ? "/app/dashboard" : "/login"} />} />
    </Routes>
  );
};

export default Routing;