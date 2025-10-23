import { useEffect, useState } from "react";
import {
  Title,
  Stack,
  Paper,
  Group,
  Button,
  Select,
  TextInput,
  Divider,
  Text,
  Card,
  LoadingOverlay
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useNavigate } from "react-router-dom";
import api, { OrderCreateDto } from "../api/api";
import { IAddress } from "../interfaces/IAddress";
import { ICart } from "../interfaces/ICart";
import { notifications } from "@mantine/notifications";

const Checkout = () => {
  const navigate = useNavigate();
  const [cart, setCart] = useState<ICart | null>(null);
  const [addresses, setAddresses] = useState<IAddress[]>([]);
  const [loading, setLoading] = useState(true);
  const [couponDiscount, setCouponDiscount] = useState(0);
  const [shippingCost, setShippingCost] = useState(0);

  const form = useForm<OrderCreateDto>({
    initialValues: {
      shippingAddressId: 0,
      billingAddressId: undefined,
      paymentMethod: '',
      couponCode: '',
      notes: ''
    },
    validate: {
      shippingAddressId: (val) => (val === 0 ? 'Please select a shipping address' : null),
      paymentMethod: (val) => (!val ? 'Please select a payment method' : null)
    }
  });

  const loadData = async () => {
    try {
      setLoading(true);
      const [cartRes, addressesRes] = await Promise.all([
        api.Cart.getCart(),
        api.Addresses.getAddresses()
      ]);

      setCart(cartRes.data);
      setAddresses(addressesRes.data);

      // Calculate shipping
      const subtotal = cartRes.data.subTotal;
      if (subtotal >= 50) {
        setShippingCost(0);
      } else {
        setShippingCost(10);
      }
    } catch (error) {
      console.error("Failed to load checkout data:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load checkout data',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleValidateCoupon = async () => {
    if (!form.values.couponCode || !cart) return;

    try {
      const response = await api.Coupons.validateCoupon({
        code: form.values.couponCode,
        orderAmount: cart.subTotal
      });

      if (response.data.isValid) {
        setCouponDiscount(response.data.discountAmount);
        notifications.show({
          title: 'Success',
          message: response.data.message,
          color: 'green',
        });
      } else {
        setCouponDiscount(0);
        notifications.show({
          title: 'Invalid Coupon',
          message: response.data.message,
          color: 'red',
        });
      }
    } catch (error) {
      setCouponDiscount(0);
      notifications.show({
        title: 'Error',
        message: 'Failed to validate coupon',
        color: 'red',
      });
    }
  };

  const handleSubmit = async (values: OrderCreateDto) => {
    try {
      setLoading(true);
      await api.Orders.createOrder(values);
      notifications.show({
        title: 'Success',
        message: 'Order placed successfully!',
        color: 'green',
      });
      navigate('/app/orders');
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to place order',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  if (loading || !cart) {
    return <LoadingOverlay visible />;
  }

  if (cart.items.length === 0) {
    navigate('/app/cart');
    return null;
  }

  const totalAmount = cart.subTotal + shippingCost - couponDiscount;

  return (
    <div style={{ padding: '20px' }}>
      <Title order={2} mb="lg">Checkout</Title>

      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Group align="flex-start" style={{ gap: '20px' }}>
          <Stack style={{ flex: 1 }}>
            <Paper withBorder p="lg">
              <Title order={4} mb="md">Shipping Address</Title>
              <Select
                label="Select Address"
                placeholder="Choose shipping address"
                required
                data={addresses.map(addr => ({
                  value: addr.id.toString(),
                  label: `${addr.firstName} ${addr.lastName} - ${addr.addressLine1}, ${addr.city}`
                }))}
                {...form.getInputProps('shippingAddressId')}
                onChange={(val) => form.setFieldValue('shippingAddressId', val ? parseInt(val) : 0)}
              />
              <Button
                variant="light"
                mt="md"
                onClick={() => navigate('/app/profile')}
              >
                Add New Address
              </Button>
            </Paper>

            <Paper withBorder p="lg">
              <Title order={4} mb="md">Payment Method</Title>
              <Select
                label="Select Payment Method"
                placeholder="Choose payment method"
                required
                data={[
                  { value: 'CreditCard', label: 'Credit Card' },
                  { value: 'DebitCard', label: 'Debit Card' },
                  { value: 'BankTransfer', label: 'Bank Transfer' },
                  { value: 'Cash', label: 'Cash on Delivery' }
                ]}
                {...form.getInputProps('paymentMethod')}
              />
            </Paper>

            <Paper withBorder p="lg">
              <Title order={4} mb="md">Coupon Code</Title>
              <Group>
                <TextInput
                  placeholder="Enter coupon code"
                  style={{ flex: 1 }}
                  {...form.getInputProps('couponCode')}
                />
                <Button onClick={handleValidateCoupon}>Apply</Button>
              </Group>
            </Paper>
          </Stack>

          <Card withBorder p="lg" style={{ width: 350 }}>
            <Title order={4} mb="md">Order Summary</Title>
            <Stack gap="xs">
              <Group justify="space-between">
                <Text>Subtotal:</Text>
                <Text>€{cart.subTotal.toFixed(2)}</Text>
              </Group>
              <Group justify="space-between">
                <Text>Shipping:</Text>
                <Text>{shippingCost === 0 ? 'FREE' : `€${shippingCost.toFixed(2)}`}</Text>
              </Group>
              {couponDiscount > 0 && (
                <Group justify="space-between" c="green">
                  <Text>Discount:</Text>
                  <Text>-€{couponDiscount.toFixed(2)}</Text>
                </Group>
              )}
              <Divider />
              <Group justify="space-between">
                <Text fw={700} size="lg">Total:</Text>
                <Text fw={700} size="lg" c="blue">
                  €{totalAmount.toFixed(2)}
                </Text>
              </Group>
              <Button fullWidth size="lg" type="submit" mt="md">
                Place Order
              </Button>
            </Stack>
          </Card>
        </Group>
      </form>
    </div>
  );
};

export default Checkout;