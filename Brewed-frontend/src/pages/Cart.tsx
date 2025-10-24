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
  Divider
} from "@mantine/core";
import { IconTrash, IconShoppingCart } from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import { ICart } from "../interfaces/ICart";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";
import { getGuestSessionId } from "../utils/guestSession";

const Cart = () => {
  const [cart, setCart] = useState<ICart | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();

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
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to update quantity',
        color: 'red',
      });
    }
  };

  const handleRemoveItem = async (cartItemId: number) => {
    try {
      await api.Cart.removeFromCart(cartItemId);
      await loadCart();
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
      <div style={{ padding: '20px', textAlign: 'center' }}>
        <IconShoppingCart size={100} color="#ccc" style={{ margin: '0 auto' }} />
        <Title order={3} mt="md" c="dimmed">Your cart is empty</Title>
        <Button mt="lg" onClick={() => navigate('/app/products')}>
          Continue Shopping
        </Button>
      </div>
    );
  }

  return (
    <div style={{ padding: '20px' }}>
      <Group justify="space-between" mb="lg">
        <Title order={2}>Shopping Cart</Title>
        <Button variant="outline" color="red" onClick={handleClearCart}>
          Clear Cart
        </Button>
      </Group>

      <Card withBorder shadow="sm" p="lg">
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
      </Card>

      <Paper withBorder p="lg" mt="lg" style={{ maxWidth: 400, marginLeft: 'auto' }}>
        <Stack>
          <Group justify="space-between">
            <Text fw={500}>Subtotal:</Text>
            <Text>€{cart.subTotal.toFixed(2)}</Text>
          </Group>
          <Divider />
          <Group justify="space-between">
            <Text fw={700} size="lg">Total:</Text>
            <Text fw={700} size="lg" c="blue">€{cart.subTotal.toFixed(2)}</Text>
          </Group>
          <Button
            fullWidth
            size="lg"
            onClick={() => navigate('/app/checkout')}
          >
            Proceed to Checkout
          </Button>
        </Stack>
      </Paper>
    </div>
  );
};

export default Cart;