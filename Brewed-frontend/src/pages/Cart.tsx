import { useEffect, useState } from "react";
import {
  Title,
  Table,
  Group,
  Button,
  Text,
  Card,
  LoadingOverlay,
  NumberInput,
  ActionIcon,
  Image,
  Stack,
  Paper,
  Divider,
  ScrollArea
} from "@mantine/core";
import { IconTrash, IconShoppingCart } from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import { ICart } from "../interfaces/ICart";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";
import useCart from "../hooks/useCart";
import { getGuestSessionId } from "../utils/guestSession";

const Cart = () => {
  const [cart, setCart] = useState<ICart | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();
  const { refreshCartCount } = useCart();

  const loadCart = async () => {
    try {
      setLoading(true);
      const sessionId = isLoggedIn ? undefined : getGuestSessionId();
      const response = await api.Cart.getCart(sessionId);
      setCart(response.data);
    } catch (error) {
      console.error("Failed to load cart:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load cart',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCart();
  }, []);

  const handleUpdateQuantity = async (cartItemId: number, quantity: number) => {
    try {
      await api.Cart.updateCartItem(cartItemId, { quantity });
      await loadCart();
      await refreshCartCount();
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to update quantity',
        color: 'red',
      });
    }
  };

  const handleRemoveItem = async (cartItemId: number) => {
    try {
      await api.Cart.removeFromCart(cartItemId);
      await loadCart();
      await refreshCartCount();
      notifications.show({
        title: 'Success',
        message: 'Item removed from cart',
        color: 'green',
      });
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to remove item',
        color: 'red',
      });
    }
  };

  const handleClearCart = async () => {
    if (window.confirm('Are you sure you want to clear your cart?')) {
      try {
        const sessionId = isLoggedIn ? undefined : getGuestSessionId();
        await api.Cart.clearCart(sessionId);
        await loadCart();
        await refreshCartCount();
        notifications.show({
          title: 'Success',
          message: 'Cart cleared',
          color: 'green',
        });
      } catch (error) {
        notifications.show({
          title: 'Error',
          message: 'Failed to clear cart',
          color: 'red',
        });
      }
    }
  };

  if (loading) {
    return <LoadingOverlay visible />;
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div style={{ textAlign: 'center', paddingTop: '60px', paddingBottom: '60px' }}>
        <div style={{
          width: 100,
          height: 100,
          borderRadius: '50%',
          background: 'linear-gradient(135deg, rgba(212, 163, 115, 0.15) 0%, rgba(139, 69, 19, 0.1) 100%)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          margin: '0 auto 20px',
        }}>
          <IconShoppingCart size={48} color="#D4A373" stroke={1.5} />
        </div>
        <Title order={3} c="dimmed" style={{ color: '#5c5c5c' }}>Your cart is empty</Title>
        <Text size="sm" c="dimmed" mt="xs" mb="lg">Browse our products to find something you love</Text>
        <Button
          onClick={() => navigate('/app/products')}
          style={{
            background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
            border: 'none',
          }}
        >
          Continue Shopping
        </Button>
      </div>
    );
  }

  return (
    <div>
      <Group justify="space-between" mb="lg">
        <div>
          <Title order={2} style={{ color: '#3d3d3d' }}>Shopping Cart</Title>
          <Text size="sm" c="dimmed" mt={4}>{cart.items.length} item{cart.items.length !== 1 ? 's' : ''}</Text>
        </div>
        <Button variant="subtle" color="red" onClick={handleClearCart}>
          Clear Cart
        </Button>
      </Group>

      <Card withBorder p="lg" style={{ borderColor: 'rgba(139, 69, 19, 0.1)' }}>
        <ScrollArea>
        <Table>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Product</Table.Th>
              <Table.Th>Price</Table.Th>
              <Table.Th>Quantity</Table.Th>
              <Table.Th>Total</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {cart.items.map((item) => (
              <Table.Tr key={item.id}>
                <Table.Td>
                  <Group>
                    <Image
                      src={item.productImageUrl}
                      alt={item.productName}
                      width={60}
                      height={60}
                      radius="md"
                    />
                    <div>
                      <Text fw={500}>{item.productName}</Text>
                      <Text size="sm" c="dimmed">
                        In stock: {item.stockQuantity}
                      </Text>
                    </div>
                  </Group>
                </Table.Td>
                <Table.Td>€{item.price.toFixed(2)}</Table.Td>
                <Table.Td>
                  <NumberInput
                    value={item.quantity}
                    onChange={(val) => handleUpdateQuantity(item.id, Number(val))}
                    min={1}
                    max={item.stockQuantity}
                    style={{ width: 80 }}
                  />
                </Table.Td>
                <Table.Td>
                  <Text fw={500}>€{item.totalPrice.toFixed(2)}</Text>
                </Table.Td>
                <Table.Td>
                  <ActionIcon
                    color="red"
                    variant="subtle"
                    onClick={() => handleRemoveItem(item.id)}
                  >
                    <IconTrash size={18} />
                  </ActionIcon>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
        </ScrollArea>
      </Card>

      <Paper
        withBorder
        p="xl"
        mt="lg"
        style={{
          maxWidth: 400,
          marginLeft: 'auto',
          borderColor: 'rgba(139, 69, 19, 0.12)',
          background: 'linear-gradient(135deg, rgba(250, 248, 245, 0.8) 0%, rgba(245, 230, 211, 0.3) 100%)',
        }}
      >
        <Stack>
          <Group justify="space-between">
            <Text fw={500} c="dimmed">Subtotal:</Text>
            <Text fw={500}>€{cart.subTotal.toFixed(2)}</Text>
          </Group>
          <Divider color="rgba(139, 69, 19, 0.1)" />
          <Group justify="space-between">
            <Text fw={700} size="lg">Total:</Text>
            <Text fw={700} size="lg" style={{ color: '#8B4513' }}>€{cart.subTotal.toFixed(2)}</Text>
          </Group>
          <Button
            fullWidth
            size="lg"
            onClick={() => navigate('/app/checkout')}
            style={{
              background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
              border: 'none',
            }}
          >
            Proceed to Checkout
          </Button>
        </Stack>
      </Paper>
    </div>
  );
};

export default Cart;