import { useEffect, useState } from "react";
import {
  Title,
  Grid,
  Text,
  Group,
  SimpleGrid,
  LoadingOverlay,
  Badge,
  Card,
} from "@mantine/core";
import {
  IconCoffee,
  IconShoppingCart,
  IconPackage,
  IconLogin,
} from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import { IProduct } from "../interfaces/IProduct";
import ProductCard from "../components/ProductCard";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";
import useCart from "../hooks/useCart";
import { getGuestSessionId } from "../utils/guestSession";
import classes from "./Dashboard.module.css";

const Dashboard = () => {
  const [loading, setLoading] = useState(true);
  const [featuredProducts, setFeaturedProducts] = useState<IProduct[]>([]);
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();
  const { refreshCartCount } = useCart();

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        const response = await api.Products.getProducts({ pageSize: 6 });
        setFeaturedProducts(response.data.items);
      } catch (error) {
        console.error("Error loading dashboard:", error);
        notifications.show({
          title: 'Error',
          message: 'Failed to load dashboard data',
          color: 'red',
        });
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const handleAddToCart = async (productId: number) => {
    try {
      const product = featuredProducts.find(p => p.id === productId);
      const sessionId = isLoggedIn ? undefined : getGuestSessionId();
      await api.Cart.addToCart({ productId, quantity: 1 }, sessionId);
      await refreshCartCount();
      notifications.show({
        title: 'Success',
        message: product ? `1 ${product.name} added to cart` : 'Product added to cart',
        color: 'green',
      });
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to add product to cart',
        color: 'red',
      });
    }
  };

  const featureCards = [
    {
      title: "Products",
      description:
        "Browse our curated selection of single-origin beans and artisan blends, freshly roasted to perfection.",
      icon: IconCoffee,
      url: "/app/products",
    },
    {
      title: "Cart",
      description:
        "Review your selected items, adjust quantities, and get ready for a seamless checkout experience.",
      icon: IconShoppingCart,
      url: "/app/cart",
    },
    {
      title: isLoggedIn ? "Orders" : "Login",
      description: isLoggedIn
        ? "Track your orders in real-time, view past purchases, and manage your delivery preferences."
        : "Sign in to your account to track orders, save favorites, and unlock exclusive rewards.",
      icon: isLoggedIn ? IconPackage : IconLogin,
      url: isLoggedIn ? "/app/orders" : "/login",
    },
  ];

  return (
    <div style={{ position: 'relative' }}>
      <LoadingOverlay visible={loading} />

      {/* Features Cards Header */}
      <div className={classes.header}>
        <Group justify="center">
          <Badge variant="filled" size="lg" style={{ background: 'rgba(212, 163, 115, 0.25)', color: '#F5E6D3', border: '1px solid rgba(212, 163, 115, 0.3)' }}>
            Premium Coffee
          </Badge>
        </Group>

        <Title order={2} className={classes.title} ta="center" mt="sm">
          Crafted for True Coffee Lovers
        </Title>

        <Text className={classes.description} ta="center" mt="md">
          Explore single-origin beans and artisan blends, freshly roasted and delivered to your door.
        </Text>

        <SimpleGrid cols={{ base: 1, md: 3 }} spacing="xl" mt={50}>
          {featureCards.map((feature) => (
            <Card
              key={feature.title}
              shadow="md"
              radius="md"
              className={classes.card}
              padding="xl"
              onClick={() => navigate(feature.url)}
            >
              <feature.icon size={50} stroke={1.5} color="#D4A373" />
              <Text fz="lg" fw={500} className={classes.cardTitle} mt="md">
                {feature.title}
              </Text>
              <Text fz="sm" c="dimmed" mt="sm">
                {feature.description}
              </Text>
            </Card>
          ))}
        </SimpleGrid>
      </div>

      {/* Featured Products Section */}
      <div style={{ marginBottom: 'var(--mantine-spacing-lg)' }}>
        <Title order={2} style={{ color: '#3d3d3d' }}>Featured Products</Title>
        <Text size="sm" c="dimmed" mt={4}>Our top picks for you</Text>
      </div>

      <Grid>
        {featuredProducts.map((product) => (
          <Grid.Col key={product.id} span={{ base: 12, sm: 6, md: 4 }}>
            <ProductCard product={product} onAddToCart={handleAddToCart} />
          </Grid.Col>
        ))}
      </Grid>
    </div>
  );
};

export default Dashboard;