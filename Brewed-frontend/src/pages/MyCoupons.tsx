import { useEffect, useState } from "react";
import {
  Title,
  Card,
  Badge,
  Text,
  Group,
  Stack,
  LoadingOverlay,
  Paper,
  SimpleGrid,
  Button
} from "@mantine/core";
import { IconTicket, IconCopy, IconCheck, IconWorld } from "@tabler/icons-react";
import api from "../api/api";
import { IUserCoupon } from "../interfaces/ICoupon";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";

const MyCoupons = () => {
  const [coupons, setCoupons] = useState<IUserCoupon[]>([]);
  const [loading, setLoading] = useState(true);
  const [copiedCode, setCopiedCode] = useState<string | null>(null);
  const { userId } = useAuth();

  useEffect(() => {
    loadCoupons();
  }, []);

  const loadCoupons = async () => {
    if (!userId) return;

    try {
      setLoading(true);
      const response = await api.Coupons.getUserCoupons(parseInt(userId));
      setCoupons(response.data);
    } catch (error) {
      console.error("Failed to load coupons:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load your coupons',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const copyToClipboard = (code: string) => {
    navigator.clipboard.writeText(code);
    setCopiedCode(code);
    notifications.show({
      title: 'Copied!',
      message: `Coupon code "${code}" copied to clipboard`,
      color: 'green',
      icon: <IconCheck size={16} />,
    });
    setTimeout(() => setCopiedCode(null), 2000);
  };

  const getDiscountText = (coupon: IUserCoupon) => {
    if (coupon.coupon.discountType === "Percentage") {
      return `${coupon.coupon.discountValue}% OFF`;
    }
    return `€${coupon.coupon.discountValue} OFF`;
  };

  const isPublicCoupon = (coupon: IUserCoupon) => coupon.id < 0;

  const isExpired = (endDate: string) => {
    return new Date(endDate) < new Date();
  };

  const isActive = (coupon: IUserCoupon) => {
    const now = new Date();
    const start = new Date(coupon.coupon.startDate);
    const end = new Date(coupon.coupon.endDate);
    return now >= start && now <= end && coupon.coupon.isActive && !coupon.isUsed;
  };

  if (loading) {
    return <LoadingOverlay visible />;
  }

  const activeCoupons = coupons.filter(c => isActive(c));
  const usedCoupons = coupons.filter(c => c.isUsed);
  const expiredCoupons = coupons.filter(c => !c.isUsed && isExpired(c.coupon.endDate));

  return (
    <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
      <Group justify="space-between" mb="xl">
        <div>
          <Title order={2}>My Coupons</Title>
          <Text c="dimmed" size="sm">Manage and use your available discount coupons</Text>
        </div>
      </Group>

      {coupons.length === 0 ? (
        <Card withBorder p="xl" style={{ textAlign: 'center' }}>
          <IconTicket size={80} color="var(--mantine-color-gray-4)" style={{ margin: '0 auto 20px' }} />
          <Title order={3} c="dimmed">No coupons available</Title>
          <Text c="dimmed" mt="sm">You don't have any coupons assigned yet. Check back later for special offers!</Text>
        </Card>
      ) : (
        <Stack gap="xl">
          {activeCoupons.length > 0 && (
            <div>
              <Title order={3} mb="md">Available Coupons</Title>
              <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }} spacing="md">
                {activeCoupons.map((userCoupon) => (
                  <Card
                    key={userCoupon.id}
                    withBorder
                    padding="lg"
                    style={{
                      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                      color: 'white',
                      position: 'relative',
                      overflow: 'hidden'
                    }}
                  >
                    <Stack gap="xs">
                      <Group justify="space-between">
                        <Group gap="xs">
                          <Badge
                            color="white"
                            variant="filled"
                            size="lg"
                            style={{ color: '#667eea' }}
                          >
                            {getDiscountText(userCoupon)}
                          </Badge>
                          {isPublicCoupon(userCoupon) && (
                            <Badge
                              color="white"
                              variant="filled"
                              size="sm"
                              style={{ color: '#764ba2' }}
                              leftSection={<IconWorld size={12} />}
                            >
                              Everyone
                            </Badge>
                          )}
                        </Group>
                        <IconTicket size={32} style={{ opacity: 0.3 }} />
                      </Group>

                      <Text fw={700} size="xl" mt="xs">
                        {userCoupon.coupon.description || 'Discount Coupon'}
                      </Text>

                      <Paper
                        p="xs"
                        style={{
                          background: 'rgba(255, 255, 255, 0.15)',
                          backdropFilter: 'blur(10px)',
                          border: '1px dashed rgba(255, 255, 255, 0.3)'
                        }}
                      >
                        <Group justify="space-between">
                          <Text fw={600} size="lg" style={{ fontFamily: 'monospace' }}>
                            {userCoupon.coupon.code}
                          </Text>
                          <Button
                            size="xs"
                            variant="white"
                            color="violet"
                            leftSection={copiedCode === userCoupon.coupon.code ? <IconCheck size={14} /> : <IconCopy size={14} />}
                            onClick={() => copyToClipboard(userCoupon.coupon.code)}
                          >
                            {copiedCode === userCoupon.coupon.code ? 'Copied' : 'Copy'}
                          </Button>
                        </Group>
                      </Paper>

                      {userCoupon.coupon.minimumOrderAmount && userCoupon.coupon.minimumOrderAmount > 0 && (
                        <Text size="xs" style={{ opacity: 0.9 }}>
                          Min. order: €{userCoupon.coupon.minimumOrderAmount}
                        </Text>
                      )}

                      <Text size="xs" style={{ opacity: 0.8 }}>
                        Valid until: {new Date(userCoupon.coupon.endDate).toLocaleDateString()}
                      </Text>
                    </Stack>
                  </Card>
                ))}
              </SimpleGrid>
            </div>
          )}

          {usedCoupons.length > 0 && (
            <div>
              <Title order={3} mb="md">Used Coupons</Title>
              <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }} spacing="md">
                {usedCoupons.map((userCoupon) => (
                  <Card key={userCoupon.id} withBorder padding="lg" style={{ opacity: 0.6 }}>
                    <Stack gap="xs">
                      <Group justify="space-between">
                        <Badge color="gray" size="lg">
                          {getDiscountText(userCoupon)}
                        </Badge>
                        <Badge color="green" variant="filled">USED</Badge>
                      </Group>

                      <Text fw={600} size="lg" c="dimmed">
                        {userCoupon.coupon.description || 'Discount Coupon'}
                      </Text>

                      <Text size="sm" c="dimmed" style={{ fontFamily: 'monospace' }}>
                        {userCoupon.coupon.code}
                      </Text>

                      <Text size="xs" c="dimmed">
                        Used on: {userCoupon.usedDate ? new Date(userCoupon.usedDate).toLocaleDateString() : 'N/A'}
                      </Text>
                    </Stack>
                  </Card>
                ))}
              </SimpleGrid>
            </div>
          )}

          {expiredCoupons.length > 0 && (
            <div>
              <Title order={3} mb="md">Expired Coupons</Title>
              <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }} spacing="md">
                {expiredCoupons.map((userCoupon) => (
                  <Card key={userCoupon.id} withBorder padding="lg" style={{ opacity: 0.5 }}>
                    <Stack gap="xs">
                      <Group justify="space-between">
                        <Badge color="gray" size="lg">
                          {getDiscountText(userCoupon)}
                        </Badge>
                        <Badge color="red" variant="filled">EXPIRED</Badge>
                      </Group>

                      <Text fw={600} size="lg" c="dimmed">
                        {userCoupon.coupon.description || 'Discount Coupon'}
                      </Text>

                      <Text size="sm" c="dimmed" style={{ fontFamily: 'monospace' }}>
                        {userCoupon.coupon.code}
                      </Text>

                      <Text size="xs" c="dimmed">
                        Expired on: {new Date(userCoupon.coupon.endDate).toLocaleDateString()}
                      </Text>
                    </Stack>
                  </Card>
                ))}
              </SimpleGrid>
            </div>
          )}
        </Stack>
      )}
    </div>
  );
};

export default MyCoupons;