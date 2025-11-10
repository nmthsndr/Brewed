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
  Select,
  TextInput,
  Pagination,
  Modal,
  Textarea
} from "@mantine/core";
import { IconPackage, IconSearch } from "@tabler/icons-react";
import api from "../api/api";
import { IOrder } from "../interfaces/IOrder";
import { notifications } from "@mantine/notifications";

const AdminOrders = () => {
  const [orders, setOrders] = useState<IOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalPages, setTotalPages] = useState(1);
  const [currentPage, setCurrentPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [selectedOrder, setSelectedOrder] = useState<IOrder | null>(null);
  const [modalOpened, setModalOpened] = useState(false);
  const [updatingStatus, setUpdatingStatus] = useState(false);
  const [generatingInvoice, setGeneratingInvoice] = useState(false);
  const [cancelModalOpened, setCancelModalOpened] = useState(false);
  const [orderNotes, setOrderNotes] = useState("");

  useEffect(() => {
    loadOrders();
  }, [currentPage, statusFilter]);

  const loadOrders = async () => {
    try {
      setLoading(true);
      const response = await api.Orders.getAllOrders(
        statusFilter || undefined,
        currentPage,
        10
      );
      setOrders(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error("Failed to load orders:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load orders',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleStatusUpdate = async (orderId: number, newStatus: string) => {
    // If cancelling, open the cancellation note modal
    if (newStatus === "Cancelled") {
      setCancelModalOpened(true);
      return;
    }

    try {
      setUpdatingStatus(true);
      await api.Orders.updateOrderStatus(orderId, newStatus);
      notifications.show({
        title: 'Success',
        message: 'Order status updated successfully',
        color: 'green',
      });
      loadOrders();
      setModalOpened(false);
    } catch (error: any) {
      console.error("Failed to update order status:", error);
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to update order status',
        color: 'red',
      });
    } finally {
      setUpdatingStatus(false);
    }
  };

  const handleCancelWithNote = async () => {
    if (!selectedOrder) return;

    if (!orderNotes.trim()) {
      notifications.show({
        title: 'Error',
        message: 'Please provide a cancellation reason',
        color: 'red',
      });
      return;
    }

    try {
      setUpdatingStatus(true);
      await api.Orders.updateOrderStatus(selectedOrder.id, "Cancelled", orderNotes);
      notifications.show({
        title: 'Success',
        message: 'Order cancelled successfully',
        color: 'green',
      });
      loadOrders();
      setCancelModalOpened(false);
      setModalOpened(false);
      setOrderNotes("");
    } catch (error: any) {
      console.error("Failed to cancel order:", error);
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to cancel order',
        color: 'red',
      });
    } finally {
      setUpdatingStatus(false);
    }
  };

  const handleGenerateInvoice = async (orderId: number) => {
    try {
      setGeneratingInvoice(true);
      await api.Orders.generateInvoice(orderId);
      notifications.show({
        title: 'Success',
        message: 'Invoice generated successfully',
        color: 'green',
      });
      loadOrders();
      // Refresh the selected order to show invoice
      if (selectedOrder) {
        const response = await api.Orders.getOrder(orderId);
        setSelectedOrder(response.data);
      }
    } catch (error: any) {
      console.error("Failed to generate invoice:", error);
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to generate invoice',
        color: 'red',
      });
    } finally {
      setGeneratingInvoice(false);
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

  const openOrderDetails = (order: IOrder) => {
    setSelectedOrder(order);
    setModalOpened(true);
  };

  if (loading && orders.length === 0) {
    return <LoadingOverlay visible />;
  }

  return (
    <div style={{ padding: '20px' }}>
      <Title order={2} mb="lg">All Orders</Title>

      <Group mb="lg">
        <Select
          placeholder="Filter by status"
          value={statusFilter}
          onChange={(value) => setStatusFilter(value || "")}
          data={[
            { value: "", label: "All Statuses" },
            { value: "Processing", label: "Processing" },
            { value: "Shipped", label: "Shipped" },
            { value: "Delivered", label: "Delivered" },
            { value: "Cancelled", label: "Cancelled" }
          ]}
          clearable
          style={{ width: 200 }}
        />
      </Group>

      {orders.length === 0 ? (
        <div style={{ padding: '40px', textAlign: 'center' }}>
          <IconPackage size={100} color="#ccc" style={{ margin: '0 auto' }} />
          <Title order={3} mt="md" c="dimmed">No orders found</Title>
          <Text c="dimmed" mt="sm">No orders match your current filters</Text>
        </div>
      ) : (
        <>
          <Card withBorder>
            <Table highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Order #</Table.Th>
                  <Table.Th>Customer</Table.Th>
                  <Table.Th>Date</Table.Th>
                  <Table.Th>Amount</Table.Th>
                  <Table.Th>Invoice</Table.Th>
                  <Table.Th>Status</Table.Th>
                  <Table.Th>Actions</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {orders.map((order) => (
                  <Table.Tr key={order.id}>
                    <Table.Td>
                      <Text size="sm" fw={500}>{order.orderNumber}</Text>
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm">{order.user?.name || 'Unknown'}</Text>
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm">
                        {new Date(order.orderDate).toLocaleDateString()}
                      </Text>
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm" fw={600}>€{order.totalAmount.toFixed(2)}</Text>
                    </Table.Td>
                    <Table.Td>
                      {order.invoice ? (
                        <Badge color="green" size="sm">Generated</Badge>
                      ) : (
                        <Badge color="yellow" size="sm">Pending</Badge>
                      )}
                    </Table.Td>
                    <Table.Td>
                      <Badge color={getStatusColor(order.status)}>
                        {order.status}
                      </Badge>
                    </Table.Td>
                    <Table.Td>
                      <Button
                        size="xs"
                        variant="light"
                        onClick={() => openOrderDetails(order)}
                      >
                        View Details
                      </Button>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Card>

          {totalPages > 1 && (
            <Group justify="center" mt="xl">
              <Pagination
                value={currentPage}
                onChange={setCurrentPage}
                total={totalPages}
              />
            </Group>
          )}
        </>
      )}

      <Modal
        opened={modalOpened}
        onClose={() => setModalOpened(false)}
        title={`Order Details - ${selectedOrder?.orderNumber}`}
        size="lg"
      >
        {selectedOrder && (
          <Stack gap="md">
            <div>
              <Text fw={600} size="sm" mb="xs">Invoice Status</Text>
              {selectedOrder.invoice ? (
                <Group gap="xs">
                  <Badge color="green">Invoice Generated</Badge>
                  <Text size="sm" c="dimmed">
                    {selectedOrder.invoice.invoiceNumber}
                  </Text>
                </Group>
              ) : (
                <Group gap="xs">
                  <Badge color="yellow">No Invoice</Badge>
                  <Button
                    size="xs"
                    variant="light"
                    onClick={() => handleGenerateInvoice(selectedOrder.id)}
                    loading={generatingInvoice}
                  >
                    Generate Invoice
                  </Button>
                </Group>
              )}
            </div>

            <div>
              <Text fw={600} size="sm">Customer Information</Text>
              <Text size="sm">Name: {selectedOrder.user?.name || 'Unknown'}</Text>
              <Text size="sm">Email: {selectedOrder.user?.email || 'Unknown'}</Text>
            </div>

            <div>
              <Text fw={600} size="sm">Order Items</Text>
              <Table mt="xs">
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Product</Table.Th>
                    <Table.Th>Quantity</Table.Th>
                    <Table.Th>Price</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {selectedOrder.items.map((item) => (
                    <Table.Tr key={item.id}>
                      <Table.Td>{item.productName}</Table.Td>
                      <Table.Td>{item.quantity}</Table.Td>
                      <Table.Td>€{item.totalPrice.toFixed(2)}</Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            </div>

            <div>
              <Text fw={600} size="sm">Shipping Address</Text>
              <Text size="sm">
                {selectedOrder.shippingAddress.firstName} {selectedOrder.shippingAddress.lastName}<br />
                {selectedOrder.shippingAddress.addressLine1}<br />
                {selectedOrder.shippingAddress.addressLine2 && (
                  <>{selectedOrder.shippingAddress.addressLine2}<br /></>
                )}
                {selectedOrder.shippingAddress.city}, {selectedOrder.shippingAddress.postalCode}<br />
                {selectedOrder.shippingAddress.country}<br />
                Phone: {selectedOrder.shippingAddress.phoneNumber}
              </Text>
            </div>

            <div>
              <Text fw={600} size="sm">Order Summary</Text>
              <Text size="sm">Subtotal: €{selectedOrder.subTotal.toFixed(2)}</Text>
              <Text size="sm">Shipping: €{selectedOrder.shippingCost.toFixed(2)}</Text>
              {selectedOrder.discount > 0 && (
                <Text size="sm" c="green">Discount: -€{selectedOrder.discount.toFixed(2)}</Text>
              )}
              <Text fw={700} size="sm">Total: €{selectedOrder.totalAmount.toFixed(2)}</Text>
            </div>

            <div>
              <Text fw={600} size="sm" mb="xs">Update Status</Text>
              <Select
                value={selectedOrder.status}
                onChange={(value) => {
                  if (value && selectedOrder) {
                    handleStatusUpdate(selectedOrder.id, value);
                  }
                }}
                data={[
                  { value: "Processing", label: "Processing" },
                  { value: "Shipped", label: "Shipped" },
                  { value: "Delivered", label: "Delivered" },
                  { value: "Cancelled", label: "Cancelled" }
                ]}
                disabled={updatingStatus}
              />
            </div>
          </Stack>
        )}
      </Modal>

      <Modal
        opened={cancelModalOpened}
        onClose={() => {
          setCancelModalOpened(false);
          setOrderNotes("");
        }}
        title="Cancel Order - Provide Reason"
        size="md"
      >
        <Stack gap="md">
          <Text size="sm" c="dimmed">
            Please provide a reason for cancelling this order. This will be sent to the customer via email.
          </Text>

          <Textarea
            label="Cancellation Reason"
            placeholder="Enter the reason for cancellation..."
            value={orderNotes}
            onChange={(e) => setOrderNotes(e.target.value)}
            minRows={4}
            maxRows={8}
            required
          />

          <Group justify="flex-end" gap="sm">
            <Button
              variant="default"
              onClick={() => {
                setCancelModalOpened(false);
                setOrderNotes("");
              }}
              disabled={updatingStatus}
            >
              Cancel
            </Button>
            <Button
              color="red"
              onClick={handleCancelWithNote}
              loading={updatingStatus}
            >
              Confirm Cancellation
            </Button>
          </Group>
        </Stack>
      </Modal>
    </div>
  );
};

export default AdminOrders;