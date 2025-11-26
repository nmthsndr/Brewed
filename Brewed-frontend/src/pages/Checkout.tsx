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
  LoadingOverlay,
  Checkbox,
  Radio
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import { IAddress } from "../interfaces/IAddress";
import { ICart } from "../interfaces/ICart";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";
import useCart from "../hooks/useCart";
import { getGuestSessionId } from "../utils/guestSession";

interface GuestOrderData {
  email: string;
  shippingAddress: {
    firstName: string;
    lastName: string;
    addressLine1: string;
    addressLine2: string;
    city: string;
    postalCode: string;
    country: string;
    phoneNumber: string;
  };
  billingAddress?: {
    firstName: string;
    lastName: string;
    addressLine1: string;
    addressLine2: string;
    city: string;
    postalCode: string;
    country: string;
    phoneNumber: string;
  };
  paymentMethod: string;
  couponCode?: string;
  notes?: string;
  useSameAddress: boolean;
}

const Checkout = () => {
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();
  const { refreshCartCount } = useCart();
  const [cart, setCart] = useState<ICart | null>(null);
  const [addresses, setAddresses] = useState<IAddress[]>([]);
  const [loading, setLoading] = useState(true);
  const [couponDiscount, setCouponDiscount] = useState(0);
  const [shippingCost, setShippingCost] = useState(0);
  const [sameAsShipping, setSameAsShipping] = useState(true);

  // For logged-in users: use existing addresses or add new inline
  const [useExistingAddress, setUseExistingAddress] = useState(true);
  const [showNewShippingForm, setShowNewShippingForm] = useState(false);
  const [showNewBillingForm, setShowNewBillingForm] = useState(false);

  // Form for logged-in users
  const loggedInForm = useForm({
    initialValues: {
      shippingAddressId: 0,
      billingAddressId: 0,
      paymentMethod: '',
      couponCode: '',
      notes: '',
      // New address fields for logged-in users
      newShippingAddress: {
        firstName: '',
        lastName: '',
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        country: '',
        phoneNumber: ''
      },
      newBillingAddress: {
        firstName: '',
        lastName: '',
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        country: '',
        phoneNumber: ''
      }
    }
  });

  // Form for guest users
  const guestForm = useForm<GuestOrderData>({
    initialValues: {
      email: '',
      shippingAddress: {
        firstName: '',
        lastName: '',
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        country: '',
        phoneNumber: ''
      },
      billingAddress: {
        firstName: '',
        lastName: '',
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        country: '',
        phoneNumber: ''
      },
      paymentMethod: '',
      couponCode: '',
      notes: '',
      useSameAddress: true
    },
    validate: {
      email: (val) => (/^\S+@\S+$/.test(val) ? null : 'Invalid email address'),
      shippingAddress: {
        firstName: (val) => (!val ? 'First name is required' : null),
        lastName: (val) => (!val ? 'Last name is required' : null),
        addressLine1: (val) => (!val ? 'Address is required' : null),
        city: (val) => (!val ? 'City is required' : null),
        postalCode: (val) => (!val ? 'Postal code is required' : null),
        country: (val) => (!val ? 'Country is required' : null),
        phoneNumber: (val) => (!val ? 'Phone number is required' : null)
      },
      billingAddress: {
        firstName: (val, values) => (!values.useSameAddress && !val ? 'First name is required' : null),
        lastName: (val, values) => (!values.useSameAddress && !val ? 'Last name is required' : null),
        addressLine1: (val, values) => (!values.useSameAddress && !val ? 'Address is required' : null),
        city: (val, values) => (!values.useSameAddress && !val ? 'City is required' : null),
        postalCode: (val, values) => (!values.useSameAddress && !val ? 'Postal code is required' : null),
        country: (val, values) => (!values.useSameAddress && !val ? 'Country is required' : null),
        phoneNumber: (val, values) => (!values.useSameAddress && !val ? 'Phone number is required' : null)
      },
      paymentMethod: (val) => (!val ? 'Please select a payment method' : null)
    }
  });

  const loadData = async () => {
    try {
      setLoading(true);
      const sessionId = isLoggedIn ? undefined : getGuestSessionId();
      const cartRes = await api.Cart.getCart(sessionId);
      setCart(cartRes.data);

      // Calculate shipping
      const subtotal = cartRes.data.subTotal;
      if (subtotal >= 50) {
        setShippingCost(0);
      } else {
        setShippingCost(10);
      }

      // Load addresses for logged-in users
      if (isLoggedIn) {
        const addressesRes = await api.Addresses.getAddresses();
        setAddresses(addressesRes.data);

        // Auto-select default address if exists
        const defaultAddress = addressesRes.data.find((addr: IAddress) => addr.isDefault);
        if (defaultAddress) {
          loggedInForm.setFieldValue('shippingAddressId', defaultAddress.id);
          loggedInForm.setFieldValue('billingAddressId', defaultAddress.id);
        }
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
  }, [isLoggedIn]);

  const handleValidateCoupon = async () => {
    if (!cart) return;

    const couponCode = isLoggedIn ? loggedInForm.values.couponCode : guestForm.values.couponCode;
    if (!couponCode) return;

    try {
      const response = await api.Coupons.validateCoupon({
        code: couponCode,
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

  const handleRemoveCoupon = () => {
    if (isLoggedIn) {
      loggedInForm.setFieldValue('couponCode', '');
    } else {
      guestForm.setFieldValue('couponCode', '');
    }
    setCouponDiscount(0);
    notifications.show({
      title: 'Coupon Removed',
      message: 'Coupon has been removed from your order',
      color: 'blue',
    });
  };

  const handleLoggedInSubmit = async (values: any) => {
    try {
      setLoading(true);

      let shippingAddressId = values.shippingAddressId;
      let billingAddressId = sameAsShipping ? values.shippingAddressId : values.billingAddressId;

      // If user chose to add new shipping address inline
      if (showNewShippingForm) {
        const newShippingRes = await api.Addresses.createAddress({
          ...values.newShippingAddress,
          isDefault: false
        });
        shippingAddressId = newShippingRes.data.id;
      }

      // If user chose to add new billing address inline and not using same as shipping
      if (!sameAsShipping && showNewBillingForm) {
        const newBillingRes = await api.Addresses.createAddress({
          ...values.newBillingAddress,
          isDefault: false
        });
        billingAddressId = newBillingRes.data.id;
      }

      const orderData = {
        shippingAddressId,
        billingAddressId,
        paymentMethod: values.paymentMethod,
        couponCode: (couponDiscount > 0 && values.couponCode?.trim()) ? values.couponCode.trim() : undefined,
        notes: values.notes || undefined
      };

      await api.Orders.createOrder(orderData);
      await refreshCartCount();

      notifications.show({
        title: 'Success',
        message: 'Order placed successfully!',
        color: 'green',
      });
      navigate('/app/orders');
    } catch (error: any) {
      console.error('Order creation error:', error);

      let errorMessage = 'Failed to place order';

      if (error.response?.data) {
        const data = error.response.data;

        if (typeof data === 'string') {
          errorMessage = data;
        } else if (data.errors) {
          // Handle .NET validation errors
          const validationErrors = Object.entries(data.errors)
            .map(([field, messages]: [string, any]) => {
              const errorList = Array.isArray(messages) ? messages : [messages];
              return `${field}: ${errorList.join(', ')}`;
            })
            .join('\n');
          errorMessage = validationErrors || data.title || 'Validation failed';
        } else if (data.message) {
          errorMessage = data.message;
        } else if (data.title) {
          errorMessage = data.title;
        }
      } else if (error.message) {
        errorMessage = error.message;
      }

      notifications.show({
        title: 'Error',
        message: errorMessage,
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleGuestSubmit = async (values: GuestOrderData) => {
    try {
      setLoading(true);

      const sessionId = getGuestSessionId();

      const guestOrderData = {
        email: values.email,
        shippingAddress: values.shippingAddress,
        billingAddress: values.useSameAddress ? values.shippingAddress : values.billingAddress,
        paymentMethod: values.paymentMethod,
        couponCode: (couponDiscount > 0 && values.couponCode?.trim()) ? values.couponCode.trim() : undefined,
        notes: values.notes || undefined,
        sessionId
      };

      await api.Orders.createGuestOrder(guestOrderData);
      await refreshCartCount();

      notifications.show({
        title: 'Success',
        message: 'Order placed successfully! Check your email for confirmation.',
        color: 'green',
      });
      navigate('/app/dashboard');
    } catch (error: any) {
      console.error('Guest order creation error:', error);

      let errorMessage = 'Failed to place order';

      if (error.response?.data) {
        const data = error.response.data;

        if (typeof data === 'string') {
          errorMessage = data;
        } else if (data.errors) {
          // Handle .NET validation errors
          const validationErrors = Object.entries(data.errors)
            .map(([field, messages]: [string, any]) => {
              const errorList = Array.isArray(messages) ? messages : [messages];
              return `${field}: ${errorList.join(', ')}`;
            })
            .join('\n');
          errorMessage = validationErrors || data.title || 'Validation failed';
        } else if (data.message) {
          errorMessage = data.message;
        } else if (data.title) {
          errorMessage = data.title;
        }
      } else if (error.message) {
        errorMessage = error.message;
      }

      notifications.show({
        title: 'Error',
        message: errorMessage,
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

  // Render address form fields
  const renderAddressFields = (prefix: string, form: any, isGuest: boolean = false) => (
    <>
      <Group grow>
        <TextInput
          label="First Name"
          placeholder="John"
          required
          {...form.getInputProps(isGuest ? `${prefix}.firstName` : `${prefix}.firstName`)}
        />
        <TextInput
          label="Last Name"
          placeholder="Doe"
          required
          {...form.getInputProps(isGuest ? `${prefix}.lastName` : `${prefix}.lastName`)}
        />
      </Group>

      <TextInput
        label="Address Line 1"
        placeholder="123 Main St"
        required
        {...form.getInputProps(isGuest ? `${prefix}.addressLine1` : `${prefix}.addressLine1`)}
      />

      <TextInput
        label="Address Line 2"
        placeholder="Apt 4B"
        {...form.getInputProps(isGuest ? `${prefix}.addressLine2` : `${prefix}.addressLine2`)}
      />

      <Group grow>
        <TextInput
          label="City"
          placeholder="Veszprém"
          required
          {...form.getInputProps(isGuest ? `${prefix}.city` : `${prefix}.city`)}
        />
        <TextInput
          label="Postal Code"
          placeholder="8200"
          required
          {...form.getInputProps(isGuest ? `${prefix}.postalCode` : `${prefix}.postalCode`)}
        />
      </Group>

      <TextInput
        label="Country"
        placeholder="Hungary"
        required
        {...form.getInputProps(isGuest ? `${prefix}.country` : `${prefix}.country`)}
      />

      <TextInput
        label="Phone Number"
        placeholder="+36 20 123 4567"
        required
        {...form.getInputProps(isGuest ? `${prefix}.phoneNumber` : `${prefix}.phoneNumber`)}
      />
    </>
  );

  // Render for guest users
  if (!isLoggedIn) {
    return (
      <div style={{ padding: '20px' }}>
        <Title order={2} mb="lg">Guest Checkout</Title>

        <form onSubmit={guestForm.onSubmit(handleGuestSubmit)}>
          <Group align="flex-start" style={{ gap: '20px' }}>
            <Stack style={{ flex: 1 }}>
              <Paper withBorder p="lg">
                <Title order={4} mb="md">Contact Information</Title>
                <TextInput
                  label="Email Address"
                  placeholder="your.email@example.com"
                  required
                  {...guestForm.getInputProps('email')}
                />
              </Paper>

              <Paper withBorder p="lg">
                <Title order={4} mb="md">Shipping Address</Title>
                <Stack>
                  {renderAddressFields('shippingAddress', guestForm, true)}
                </Stack>
              </Paper>

              <Paper withBorder p="lg">
                <Title order={4} mb="md">Billing Address</Title>
                <Checkbox
                  label="Same as shipping address"
                  checked={guestForm.values.useSameAddress}
                  onChange={(e) => {
                    guestForm.setFieldValue('useSameAddress', e.currentTarget.checked);
                    setSameAsShipping(e.currentTarget.checked);
                  }}
                  mb="md"
                />
                {!guestForm.values.useSameAddress && (
                  <Stack>
                    {renderAddressFields('billingAddress', guestForm, true)}
                  </Stack>
                )}
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
                  {...guestForm.getInputProps('paymentMethod')}
                />
              </Paper>

              <Paper withBorder p="lg">
                <Title order={4} mb="md">Coupon Code (Optional)</Title>
                <Group>
                  <TextInput
                    placeholder="Enter coupon code"
                    style={{ flex: 1 }}
                    {...guestForm.getInputProps('couponCode')}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        e.preventDefault();
                        handleValidateCoupon();
                      }
                    }}
                  />
                  <Button onClick={handleValidateCoupon}>Apply</Button>
                  {couponDiscount > 0 && (
                    <Button onClick={handleRemoveCoupon} color="red" variant="light">
                      Remove
                    </Button>
                  )}
                </Group>
                {couponDiscount > 0 && (
                  <Text size="sm" c="green" mt="xs">
                    Coupon applied! Discount: €{couponDiscount.toFixed(2)}
                  </Text>
                )}
              </Paper>
            </Stack>

            <Card withBorder p="lg" style={{ width: 350, position: 'sticky', top: 20 }}>
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
  }

  // Render for logged-in users
  return (
    <div style={{ padding: '20px' }}>
      <Title order={2} mb="lg">Checkout</Title>

      <form onSubmit={loggedInForm.onSubmit(handleLoggedInSubmit)}>
        <Group align="flex-start" style={{ gap: '20px' }}>
          <Stack style={{ flex: 1 }}>
            <Paper withBorder p="lg">
              <Title order={4} mb="md">Shipping Address</Title>

              {addresses.length > 0 && !showNewShippingForm ? (
                <>
                  <Select
                    label="Select Address"
                    placeholder="Choose shipping address"
                    required
                    data={addresses.map(addr => ({
                      value: addr.id.toString(),
                      label: `${addr.firstName} ${addr.lastName} - ${addr.addressLine1}, ${addr.city}`
                    }))}
                    value={loggedInForm.values.shippingAddressId > 0 ? loggedInForm.values.shippingAddressId.toString() : null}
                    onChange={(val) => loggedInForm.setFieldValue('shippingAddressId', val ? parseInt(val) : 0)}
                  />
                  <Button
                    variant="light"
                    mt="md"
                    onClick={() => setShowNewShippingForm(true)}
                  >
                    + Add New Address
                  </Button>
                </>
              ) : (
                <Stack>
                  {renderAddressFields('newShippingAddress', loggedInForm, false)}
                  {addresses.length > 0 && (
                    <Button
                      variant="light"
                      onClick={() => setShowNewShippingForm(false)}
                    >
                      Use Existing Address
                    </Button>
                  )}
                </Stack>
              )}
            </Paper>

            <Paper withBorder p="lg">
              <Title order={4} mb="md">Billing Address</Title>
              <Checkbox
                label="Same as shipping address"
                checked={sameAsShipping}
                onChange={(e) => setSameAsShipping(e.currentTarget.checked)}
                mb="md"
              />
              {!sameAsShipping && (
                addresses.length > 0 && !showNewBillingForm ? (
                  <>
                    <Select
                      label="Select Billing Address"
                      placeholder="Choose billing address"
                      required
                      data={addresses.map(addr => ({
                        value: addr.id.toString(),
                        label: `${addr.firstName} ${addr.lastName} - ${addr.addressLine1}, ${addr.city}`
                      }))}
                      value={loggedInForm.values.billingAddressId?.toString()}
                      onChange={(val) => loggedInForm.setFieldValue('billingAddressId', val ? parseInt(val) : 0)}
                    />
                    <Button
                      variant="light"
                      mt="md"
                      onClick={() => setShowNewBillingForm(true)}
                    >
                      + Add New Address
                    </Button>
                  </>
                ) : (
                  <Stack>
                    {renderAddressFields('newBillingAddress', loggedInForm, false)}
                    {addresses.length > 0 && (
                      <Button
                        variant="light"
                        onClick={() => setShowNewBillingForm(false)}
                      >
                        Use Existing Address
                      </Button>
                    )}
                  </Stack>
                )
              )}
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
                {...loggedInForm.getInputProps('paymentMethod')}
              />
            </Paper>
{/*
            <Paper withBorder p="lg">
              <Title order={4} mb="md">Coupon Code (Optional)</Title>
              <Group>
                <TextInput
                  placeholder="Enter coupon code"
                  style={{ flex: 1 }}
                  {...loggedInForm.getInputProps('couponCode')}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault();
                      handleValidateCoupon();
                    }
                  }}
                />
                <Button onClick={handleValidateCoupon}>Apply</Button>
                {couponDiscount > 0 && (
                  <Button onClick={handleRemoveCoupon} color="red" variant="light">
                    Remove
                  </Button>
                )}
              </Group>
              {couponDiscount > 0 && (
                <Text size="sm" c="green" mt="xs">
                  Coupon applied! Discount: €{couponDiscount.toFixed(2)}
                </Text>
              )}
            </Paper>
          */}

          </Stack>

          <Card withBorder p="lg" style={{ width: 350, position: 'sticky', top: 20 }}>
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