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
import useCart from "../hooks/useCart";
import { getGuestSessionId } from "../utils/guestSession";

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

  return (
    <div style={{ position: 'relative' }}>
      <LoadingOverlay visible={loading} />

      {/* Hero Section */}
      <div style={{
        background: 'linear-gradient(135deg, #8B4513 0%, #6B3410 50%, #4a240b 100%)',
        borderRadius: '20px',
        padding: '48px 36px',
        marginBottom: '32px',
        position: 'relative',
        overflow: 'hidden',
      }}>
        <div style={{
          position: 'absolute',
          top: 0,
          right: 0,
          bottom: 0,
          width: '40%',
          background: 'radial-gradient(circle at 70% 50%, rgba(212, 163, 115, 0.15) 0%, transparent 70%)',
        }} />
        <Title
          order={1}
          style={{
            color: '#F5E6D3',
            fontSize: 'clamp(1.8rem, 4vw, 2.5rem)',
            marginBottom: '8px',
            position: 'relative',
            zIndex: 1,
          }}
        >
          Welcome to Brewed
        </Title>
        <Text
          size="lg"
          style={{
            color: 'rgba(245, 230, 211, 0.75)',
            maxWidth: '500px',
            position: 'relative',
            zIndex: 1,
          }}
        >
          Discover our handpicked selection of premium coffees from around the world.
        </Text>
      </div>

      {/* Quick Action Cards */}
      <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="lg" mb="xl">
        <Paper
          withBorder
          p="lg"
          radius="lg"
          style={{
            cursor: 'pointer',
            borderColor: 'rgba(139, 69, 19, 0.1)',
            transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = 'translateY(-4px)';
            e.currentTarget.style.boxShadow = '0 8px 24px rgba(139, 69, 19, 0.1)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '';
          }}
          onClick={() => navigate('/app/products')}
        >
          <Group>
            <div style={{
              width: 52,
              height: 52,
              borderRadius: '14px',
              background: 'linear-gradient(135deg, rgba(212, 163, 115, 0.2) 0%, rgba(139, 69, 19, 0.15) 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}>
              <IconShoppingBag size={28} color="#8B4513" stroke={1.5} />
            </div>
            <div>
              <Text size="xs" c="dimmed" tt="uppercase" fw={700} style={{ letterSpacing: '0.05em' }}>
                Browse
              </Text>
              <Text fw={700} size="xl" style={{ color: '#3d3d3d' }}>Products</Text>
            </div>
          </Group>
        </Paper>

        <Paper
          withBorder
          p="lg"
          radius="lg"
          style={{
            cursor: 'pointer',
            borderColor: 'rgba(139, 69, 19, 0.1)',
            transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = 'translateY(-4px)';
            e.currentTarget.style.boxShadow = '0 8px 24px rgba(139, 69, 19, 0.1)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '';
          }}
          onClick={() => navigate('/app/cart')}
        >
          <Group>
            <div style={{
              width: 52,
              height: 52,
              borderRadius: '14px',
              background: 'linear-gradient(135deg, rgba(212, 163, 115, 0.2) 0%, rgba(139, 69, 19, 0.15) 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}>
              <IconShoppingCart size={28} color="#8B4513" stroke={1.5} />
            </div>
            <div>
              <Text size="xs" c="dimmed" tt="uppercase" fw={700} style={{ letterSpacing: '0.05em' }}>
                View
              </Text>
              <Text fw={700} size="xl" style={{ color: '#3d3d3d' }}>Cart</Text>
            </div>
          </Group>
        </Paper>

        {isLoggedIn ? (
          <Paper
            withBorder
            p="lg"
            radius="lg"
            style={{
              cursor: 'pointer',
              borderColor: 'rgba(139, 69, 19, 0.1)',
              transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = 'translateY(-4px)';
              e.currentTarget.style.boxShadow = '0 8px 24px rgba(139, 69, 19, 0.1)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = 'translateY(0)';
              e.currentTarget.style.boxShadow = '';
            }}
            onClick={() => navigate('/app/orders')}
          >
            <Group>
              <div style={{
                width: 52,
                height: 52,
                borderRadius: '14px',
                background: 'linear-gradient(135deg, rgba(212, 163, 115, 0.2) 0%, rgba(139, 69, 19, 0.15) 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}>
                <IconPackage size={28} color="#8B4513" stroke={1.5} />
              </div>
              <div>
                <Text size="xs" c="dimmed" tt="uppercase" fw={700} style={{ letterSpacing: '0.05em' }}>
                  Track
                </Text>
                <Text fw={700} size="xl" style={{ color: '#3d3d3d' }}>Orders</Text>
              </div>
            </Group>
          </Paper>
        ) : (
          <Paper
            withBorder
            p="lg"
            radius="lg"
            style={{
              cursor: 'pointer',
              borderColor: 'rgba(139, 69, 19, 0.1)',
              transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = 'translateY(-4px)';
              e.currentTarget.style.boxShadow = '0 8px 24px rgba(139, 69, 19, 0.1)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = 'translateY(0)';
              e.currentTarget.style.boxShadow = '';
            }}
            onClick={() => navigate('/login')}
          >
            <Group>
              <div style={{
                width: 52,
                height: 52,
                borderRadius: '14px',
                background: 'linear-gradient(135deg, rgba(212, 163, 115, 0.2) 0%, rgba(139, 69, 19, 0.15) 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}>
                <IconLogin size={28} color="#8B4513" stroke={1.5} />
              </div>
              <div>
                <Text size="xs" c="dimmed" tt="uppercase" fw={700} style={{ letterSpacing: '0.05em' }}>
                  Sign In
                </Text>
                <Text fw={700} size="xl" style={{ color: '#3d3d3d' }}>Login</Text>
              </div>
            </Group>
          </Paper>
        )}
      </SimpleGrid>

      {/* Featured Products Section */}
      <Group justify="space-between" mb="lg" align="baseline">
        <div>
          <Title order={2} style={{ color: '#3d3d3d' }}>Featured Products</Title>
          <Text size="sm" c="dimmed" mt={4}>Our top picks for you</Text>
        </div>
        <Button
          variant="subtle"
          color="brown"
          onClick={() => navigate('/app/products')}
          style={{ fontWeight: 600 }}
        >
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