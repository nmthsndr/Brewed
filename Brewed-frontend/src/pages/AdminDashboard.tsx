import { useEffect, useState } from "react";
import {
  Title,
  SimpleGrid,
  Paper,
  Text,
  Group,
  LoadingOverlay,
  Table,
  Badge,
  Card,
  Stack,
  RingProgress,
  Center
} from "@mantine/core";
import { IconCurrencyDollar, IconShoppingCart, IconUsers, IconPackage } from "@tabler/icons-react";
import api from "../api/api";
import { notifications } from "@mantine/notifications";

interface DashboardStats {
  totalRevenue: number;
  monthlyRevenue: number;
  totalOrders: number;
  monthlyOrders: number;
  totalCustomers: number;
  totalProducts: number;
  lowStockProducts: number;
  averageOrderValue: number;
  topProducts: any[];
  recentOrders: any[];
}

const AdminDashboard = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async () => {
    try {
      setLoading(true);
      const response = await api.Dashboard.getStats();
      setStats(response.data);
    } catch (error) {
      console.error("Failed to load dashboard stats:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load dashboard statistics',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  if (loading || !stats) {
    return <LoadingOverlay visible />;
  }

  const statCards = [
    {
      title: 'Total Revenue',
      value: `€${stats.totalRevenue.toFixed(0)}`,
      icon: IconCurrencyDollar,
      color: 'blue',
      description: `€${stats.monthlyRevenue.toFixed(0)} this month`
    },
    {
      title: 'Total Orders',
      value: stats.totalOrders,
      icon: IconShoppingCart,
      color: 'green',
      description: `${stats.monthlyOrders} this month`
    },
    {
      title: 'Customers',
      value: stats.totalCustomers,
      icon: IconUsers,
      color: 'violet',
      description: 'Registered users'
    },
    {
      title: 'Products',
      value: stats.totalProducts,
      icon: IconPackage,
      color: 'orange',
      description: `${stats.lowStockProducts} low stock`
    }
  ];

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Processing': return 'blue';
      case 'Shipped': return 'cyan';
      case 'Delivered': return 'green';
      case 'Cancelled': return 'red';
      default: return 'gray';
    }
  };

  return (
    <div>
      <Title order={2} mb="xs" style={{ color: '#3d3d3d' }}>Admin Dashboard</Title>
      <Text size="sm" c="dimmed" mb="xl">Overview of your store's performance</Text>

      <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }} spacing="lg" mb="xl">
        {statCards.map((stat) => (
          <Paper
            key={stat.title}
            withBorder
            p="lg"
            radius="lg"
            style={{
              borderColor: 'rgba(139, 69, 19, 0.1)',
              transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = 'translateY(-3px)';
              e.currentTarget.style.boxShadow = '0 6px 20px rgba(139, 69, 19, 0.08)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = 'translateY(0)';
              e.currentTarget.style.boxShadow = '';
            }}
          >
            <Group justify="space-between">
              <div>
                <Text c="dimmed" size="xs" tt="uppercase" fw={700} style={{ letterSpacing: '0.05em' }}>
                  {stat.title}
                </Text>
                <Text fw={700} size="xl" mt="xs" style={{ color: '#3d3d3d' }}>
                  {stat.value}
                </Text>
                <Text c="dimmed" size="xs" mt="xs">
                  {stat.description}
                </Text>
              </div>
              <div style={{
                width: 48,
                height: 48,
                borderRadius: '14px',
                background: `linear-gradient(135deg, var(--mantine-color-${stat.color}-1), var(--mantine-color-${stat.color}-2))`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}>
                <stat.icon size={24} color={`var(--mantine-color-${stat.color}-6)`} stroke={1.5} />
              </div>
            </Group>
          </Paper>
        ))}
      </SimpleGrid>

      <SimpleGrid cols={{ base: 1, lg: 2 }} spacing="lg" mb="xl">
        <Card withBorder style={{ borderColor: 'rgba(139, 69, 19, 0.1)' }}>
          <Title order={4} mb="md" style={{ color: '#3d3d3d' }}>Top Products</Title>
          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Product</Table.Th>
                <Table.Th>Sold</Table.Th>
                <Table.Th>Revenue</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {stats.topProducts.map((product) => (
                <Table.Tr key={product.productId}>
                  <Table.Td>
                    <Text size="sm" fw={500}>{product.productName}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge>{product.totalSold}</Badge>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">€{product.totalRevenue.toFixed(0)}</Text>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>

        <Card withBorder style={{ borderColor: 'rgba(139, 69, 19, 0.1)' }}>
          <Title order={4} mb="md" style={{ color: '#3d3d3d' }}>Recent Orders</Title>
          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Order #</Table.Th>
                <Table.Th>Customer</Table.Th>
                <Table.Th>Amount</Table.Th>
                <Table.Th>Status</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {stats.recentOrders.map((order) => (
                <Table.Tr key={order.orderId}>
                  <Table.Td>
                    <Text size="sm" fw={500}>{order.orderNumber}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{order.customerName}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">€{order.totalAmount.toFixed(0)}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={getStatusColor(order.status)}>
                      {order.status}
                    </Badge>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>
      </SimpleGrid>

      <Paper withBorder p="lg" radius="lg" style={{ borderColor: 'rgba(139, 69, 19, 0.1)' }}>
        <Title order={4} mb="md" style={{ color: '#3d3d3d' }}>Quick Stats</Title>
        <SimpleGrid cols={{ base: 1, sm: 3 }}>
          <Center>
            <Stack align="center">
              <RingProgress
                size={120}
                thickness={12}
                sections={[
                  { value: (stats.monthlyRevenue / stats.totalRevenue) * 100, color: 'blue' }
                ]}
                label={
                  <Center>
                    <Text size="xs" ta="center">
                      Monthly
                      <br />
                      Revenue
                    </Text>
                  </Center>
                }
              />
              <Text size="sm" c="dimmed">
                {((stats.monthlyRevenue / stats.totalRevenue) * 100).toFixed(1)}% of total
              </Text>
            </Stack>
          </Center>

          <Center>
            <Stack align="center">
              <RingProgress
                size={120}
                thickness={12}
                sections={[
                  { value: (stats.monthlyOrders / stats.totalOrders) * 100, color: 'green' }
                ]}
                label={
                  <Center>
                    <Text size="xs" ta="center">
                      Monthly
                      <br />
                      Orders
                    </Text>
                  </Center>
                }
              />
              <Text size="sm" c="dimmed">
                {((stats.monthlyOrders / stats.totalOrders) * 100).toFixed(1)}% of total
              </Text>
            </Stack>
          </Center>

          <Center>
            <Stack align="center">
              <RingProgress
                size={120}
                thickness={12}
                sections={[
                  { value: ((stats.totalProducts - stats.lowStockProducts) / stats.totalProducts) * 100, color: 'orange' }
                ]}
                label={
                  <Center>
                    <Text size="xs" ta="center">
                      Stock
                      <br />
                      Health
                    </Text>
                  </Center>
                }
              />
              <Text size="sm" c="dimmed">
                {stats.lowStockProducts} low stock items
              </Text>
            </Stack>
          </Center>
        </SimpleGrid>
      </Paper>
    </div>
  );
};

export default AdminDashboard;