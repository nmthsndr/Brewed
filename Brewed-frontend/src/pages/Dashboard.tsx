import { useEffect, useState } from "react";
import {
  Title,
  Grid,
  Card,
  Text,
  Group,
  SimpleGrid,
  Paper,
  LoadingOverlay,
  Button
} from "@mantine/core";
import { IconShoppingBag, IconShoppingCart, IconPackage, IconLogin } from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import { IProduct } from "../interfaces/IProduct";
import ProductCard from "../components/ProductCard";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";
import { getGuestSessionId } from "../utils/guestSession";

const Dashboard = () => {
  const [loading, setLoading] = useState(true);
  const [featuredProducts, setFeaturedProducts] = useState<IProduct[]>([]);
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();

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
      notifications.show({
        title: 'Success',
        message: product ? `1 ${product.name} added to cart` : 'Product added to cart',
        color: 'green',
      });
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to add product to cart',
        color: 'red',
      });
    }
  };

  return (
    <div style={{ padding: '20px', position: 'relative' }}>
      <LoadingOverlay visible={loading} />

      <Title order={2} mb="xl">Welcome to Brewed Coffee!</Title>

      <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="lg" mb="xl">
        <Paper withBorder p="md" radius="md" style={{ cursor: 'pointer' }} onClick={() => navigate('/app/products')}>
          <Group>
            <IconShoppingBag size={40} color="#228be6" />
            <div>
              <Text size="xs" c="dimmed" tt="uppercase" fw={700}>
                Browse
              </Text>
              <Text fw={700} size="xl">Products</Text>
            </div>
          </Group>
        </Paper>

        <Paper withBorder p="md" radius="md" style={{ cursor: 'pointer' }} onClick={() => navigate('/app/cart')}>
          <Group>
            <IconShoppingCart size={40} color="#228be6" />
            <div>
              <Text size="xs" c="dimmed" tt="uppercase" fw={700}>
                View
              </Text>
              <Text fw={700} size="xl">Cart</Text>
            </div>
          </Group>
        </Paper>

        {isLoggedIn ? (
          <Paper withBorder p="md" radius="md" style={{ cursor: 'pointer' }} onClick={() => navigate('/app/orders')}>
            <Group>
              <IconPackage size={40} color="#228be6" />
              <div>
                <Text size="xs" c="dimmed" tt="uppercase" fw={700}>
                  Track
                </Text>
                <Text fw={700} size="xl">Orders</Text>
              </div>
            </Group>
          </Paper>
        ) : (
          <Paper withBorder p="md" radius="md" style={{ cursor: 'pointer' }} onClick={() => navigate('/login')}>
            <Group>
              <IconLogin size={40} color="#228be6" />
              <div>
                <Text size="xs" c="dimmed" tt="uppercase" fw={700}>
                  Sign In
                </Text>
                <Text fw={700} size="xl">Login</Text>
              </div>
            </Group>
          </Paper>
        )}
      </SimpleGrid>

      <Group justify="space-between" mb="md">
        <Title order={3}>Featured Products</Title>
        <Button variant="light" onClick={() => navigate('/app/products')}>
          View All
        </Button>
      </Group>

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