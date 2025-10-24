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
import Coupons from "../pages/Coupons";
import AdminDashboard from "../pages/AdminDashboard";
import Users from "../pages/Users";
import AdminProducts from "../pages/AdminProducts";
import AdminOrders from "../pages/AdminOrders";
import AdminReviews from "../pages/AdminReviews";
import ResetPassword from "../pages/ResetPassword";

const Routing = () => {
  const { isLoggedIn, role } = useAuth();

  return (
    <Routes>
      {/* Public Routes */}
      <Route path="/login" element={!isLoggedIn ? <Login /> : <Navigate to="/app/dashboard" />} />
      <Route path="/register" element={!isLoggedIn ? <Register /> : <Navigate to="/app/dashboard" />} />
      <Route path="/forgot-password" element={!isLoggedIn ? <ForgotPassword /> : <Navigate to="/app/dashboard" />} />
      <Route path="/reset-password" element={!isLoggedIn ? <ResetPassword /> : <Navigate to="/app/dashboard" />} />
      <Route path="/confirm-email" element={<ConfirmEmail />} />

      {/* App Routes - Accessible to both guests and logged-in users */}
      <Route path="/app" element={<BasicLayout />}>
        {/* Public pages - accessible to guests */}
        <Route path="dashboard" element={<Dashboard />} />
        <Route path="products" element={<Products />} />
        <Route path="products/:id" element={<ProductDetail />} />
        <Route path="cart" element={<Cart />} />
        <Route path="checkout" element={<Checkout />} />

        {/* Protected pages - require login */}
        <Route path="orders" element={isLoggedIn ? <Orders /> : <Navigate to="/login" />} />
        <Route path="profile" element={isLoggedIn ? <Profile /> : <Navigate to="/login" />} />

        {/* Admin Only Routes */}
        {role === 'Admin' && (
          <>
            <Route path="admin-products" element={<AdminProducts />} />
            <Route path="categories" element={<Categories />} />
            <Route path="coupons" element={<Coupons />} />
            <Route path="admin-dashboard" element={<AdminDashboard />} />
            <Route path="admin-orders" element={<AdminOrders />} />
            <Route path="admin-reviews" element={<AdminReviews />} />
            <Route path="users" element={<Users />} />
          </>
        )}
      </Route>

      {/* Default Route - always redirect to dashboard */}
      <Route path="/" element={<Navigate to="/app/dashboard" />} />
      <Route path="*" element={<Navigate to="/app/dashboard" />} />
    </Routes>
  );
};

export default Routing;