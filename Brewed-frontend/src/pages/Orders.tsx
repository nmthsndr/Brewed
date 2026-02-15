import { useEffect, useState } from "react";
import {
  Title,
  Table,
  Badge,
  Group,
  Text,
  Card,
  LoadingOverlay,
  Button,
  Stack,
  Accordion,
  ScrollArea
} from "@mantine/core";
import { IconPackage, IconDownload, IconAlertCircle } from "@tabler/icons-react";
import { notifications } from "@mantine/notifications";
import api from "../api/api";
import { IOrder } from "../interfaces/IOrder";

const Orders = () => {
  const [orders, setOrders] = useState<IOrder[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadOrders();
  }, []);

  const loadOrders = async () => {
    try {
      setLoading(true);
      const response = await api.Orders.getOrders();
      setOrders(response.data);
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to load orders',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Processing': return 'blue';
      case 'Shipped': return 'cyan';
      case 'Delivered': return 'green';
      case 'Cancelled': return 'red';
      default: return 'gray';
    }
  };

  if (loading) {
    return <LoadingOverlay visible />;
  }

  if (orders.length === 0) {
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
          <IconPackage size={48} color="#D4A373" stroke={1.5} />
        </div>
        <Title order={3} style={{ color: '#5c5c5c' }}>No orders yet</Title>
        <Text c="dimmed" mt="xs">Your orders will appear here</Text>
      </div>
    );
  }

  return (
    <div>
      <Title order={2} mb="xs" style={{ color: '#3d3d3d' }}>My Orders</Title>
      <Text size="sm" c="dimmed" mb="lg">{orders.length} order{orders.length !== 1 ? 's' : ''}</Text>

      <Stack gap="md">
        {orders.map((order) => (
          <Card key={order.id} withBorder p="lg" style={{ borderColor: 'rgba(139, 69, 19, 0.1)' }}>
            <Accordion>
              <Accordion.Item value={order.id.toString()}>
                <Accordion.Control>
                  <Group justify="space-between" wrap="wrap" gap="xs">
                    <div>
                      <Text fw={700}>{order.orderNumber}</Text>
                      <Text size="sm" c="dimmed">
                        {new Date(order.orderDate).toLocaleDateString()}
                      </Text>
                    </div>
                    <Group gap="xs" wrap="wrap">
                      <Badge color={getStatusColor(order.status)}>
                        {order.status}
                      </Badge>
                      <Text fw={700}>€{order.totalAmount.toFixed(2)}</Text>
                    </Group>
                  </Group>
                </Accordion.Control>
                <Accordion.Panel>
                  <Stack gap="sm">
                    <Text fw={600}>Order Items:</Text>
                    <ScrollArea>
                    <Table>
                      <Table.Thead>
                        <Table.Tr>
                          <Table.Th>Product</Table.Th>
                          <Table.Th>Quantity</Table.Th>
                          <Table.Th>Price</Table.Th>
                        </Table.Tr>
                      </Table.Thead>
                      <Table.Tbody>
                        {order.items.map((item) => (
                          <Table.Tr key={item.id}>
                            <Table.Td>{item.productName}</Table.Td>
                            <Table.Td>{item.quantity}</Table.Td>
                            <Table.Td>€{item.totalPrice.toFixed(2)}</Table.Td>
                          </Table.Tr>
                        ))}
                      </Table.Tbody>
                    </Table>
                    </ScrollArea>

                    {order.status === 'Cancelled' && order.notes && (
                      <Card
                        withBorder
                        p="md"
                        mt="md"
                        style={{
                          backgroundColor: '#fff3cd',
                          borderLeft: '4px solid #ffc107'
                        }}
                      >
                        <Group gap="xs" align="flex-start">
                          <IconAlertCircle size={20} color="#856404" />
                          <div style={{ flex: 1 }}>
                            <Text fw={600} size="sm" c="#856404">Cancellation Reason:</Text>
                            <Text size="sm" c="#856404" mt="xs">{order.notes}</Text>
                          </div>
                        </Group>
                      </Card>
                    )}

                    <Text fw={600} mt="md">Shipping Address:</Text>
                    <Text size="sm">
                      {order.shippingAddress.firstName} {order.shippingAddress.lastName}<br />
                      {order.shippingAddress.addressLine1}<br />
                      {order.shippingAddress.city}, {order.shippingAddress.postalCode}<br />
                      {order.shippingAddress.country}
                    </Text>

                    <Group justify="space-between" mt="md">
                      <div>
                        <Text size="sm">Subtotal: €{order.subTotal.toFixed(2)}</Text>
                        <Text size="sm">Shipping: €{order.shippingCost.toFixed(2)}</Text>
                        {order.discount > 0 && (
                          <Text size="sm" c="green">Discount: -€{order.discount.toFixed(2)}</Text>
                        )}
                        <Text fw={700}>Total: €{order.totalAmount.toFixed(2)}</Text>
                      </div>
                      {order.invoice && (
                        <Button
                          leftSection={<IconDownload size={16} />}
                          variant="light"
                          color="blue"
                          onClick={async () => {
                            try {
                              const response = await api.Orders.getInvoicePdf(order.id);
                              const blob = new Blob([response.data], { type: 'application/pdf' });
                              const url = window.URL.createObjectURL(blob);
                              const link = document.createElement('a');
                              link.href = url;
                              link.download = `invoice-${order.invoice.invoiceNumber}.pdf`;
                              document.body.appendChild(link);
                              link.click();
                              document.body.removeChild(link);
                              window.URL.revokeObjectURL(url);
                            } catch (error) {
                              console.error('Failed to download invoice:', error);
                              notifications.show({
                                title: 'Error',
                                message: 'Failed to download invoice',
                                color: 'red',
                              });
                            }
                          }}
                        >
                          Download Invoice
                        </Button>
                      )}
                    </Group>
                  </Stack>
                </Accordion.Panel>
              </Accordion.Item>
            </Accordion>
          </Card>
        ))}
      </Stack>
    </div>
  );
};

export default Orders;