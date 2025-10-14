import { useEffect, useState } from "react";
import {
  Title,
  Table,
  Group,
  Button,
  ActionIcon,
  Text,
  Modal,
  TextInput,
  Stack,
  LoadingOverlay,
  Badge,
  Select,
  NumberInput,
  Switch
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { DateInput } from "@mantine/dates";
import { IconEdit, IconTrash, IconPlus } from "@tabler/icons-react";
import api from "../api/api";
import { ICoupon } from "../interfaces/ICoupon";
import { notifications } from "@mantine/notifications";

const Coupons = () => {
  const [coupons, setCoupons] = useState<ICoupon[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedCoupon, setSelectedCoupon] = useState<ICoupon | null>(null);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [opened, { open, close }] = useDisclosure(false);

  const form = useForm({
    initialValues: {
      code: '',
      description: '',
      discountType: 'Percentage',
      discountValue: 0,
      minimumOrderAmount: 0,
      startDate: new Date(),
      endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000),
      isActive: true
    },
    validate: {
      code: (value) => !value ? 'Coupon code is required' : null,
      discountValue: (value) => value <= 0 ? 'Discount value must be positive' : null
    }
  });

  const loadCoupons = async () => {
    try {
      setLoading(true);
      const response = await api.Coupons.getCoupons();
      setCoupons(response.data);
    } catch (error) {
      console.error("Failed to load coupons:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load coupons',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCoupons();
  }, []);

  const handleCreate = () => {
    setModalMode('create');
    form.reset();
    open();
  };

  const handleEdit = (coupon: ICoupon) => {
    setModalMode('edit');
    setSelectedCoupon(coupon);
    form.setValues({
      code: coupon.code,
      description: coupon.description,
      discountType: coupon.discountType,
      discountValue: coupon.discountValue,
      minimumOrderAmount: coupon.minimumOrderAmount || 0,
      startDate: new Date(coupon.startDate),
      endDate: new Date(coupon.endDate),
      isActive: coupon.isActive
    });
    open();
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this coupon?')) {
      try {
        setLoading(true);
        await api.Coupons.deleteCoupon(id);
        await loadCoupons();
        notifications.show({
          title: 'Success',
          message: 'Coupon deleted successfully',
          color: 'green',
        });
      } catch (error) {
        notifications.show({
          title: 'Error',
          message: 'Failed to delete coupon',
          color: 'red',
        });
      } finally {
        setLoading(false);
      }
    }
  };

  const handleSubmit = async (values: typeof form.values) => {
    try {
      setLoading(true);

      const couponData = {
        ...values,
        minimumOrderAmount: values.minimumOrderAmount || undefined
      };

      if (modalMode === 'create') {
        await api.Coupons.createCoupon(couponData);
        notifications.show({
          title: 'Success',
          message: 'Coupon created successfully',
          color: 'green',
        });
      } else if (selectedCoupon) {
        await api.Coupons.updateCoupon(selectedCoupon.id, couponData);
        notifications.show({
          title: 'Success',
          message: 'Coupon updated successfully',
          color: 'green',
        });
      }

      await loadCoupons();
      close();
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to save coupon',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '20px', position: 'relative' }}>
      <LoadingOverlay visible={loading} />

      <Group justify="space-between" mb="lg">
        <Title order={2}>Coupons Management</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={handleCreate}>
          Add Coupon
        </Button>
      </Group>

      {coupons.length === 0 ? (
        <Text ta="center" c="dimmed">No coupons found</Text>
      ) : (
        <Table striped highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Code</Table.Th>
              <Table.Th>Description</Table.Th>
              <Table.Th>Discount</Table.Th>
              <Table.Th>Min Order</Table.Th>
              <Table.Th>Valid Until</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {coupons.map((coupon) => (
              <Table.Tr key={coupon.id}>
                <Table.Td>
                  <Text fw={600} c="blue">{coupon.code}</Text>
                </Table.Td>
                <Table.Td>{coupon.description}</Table.Td>
                <Table.Td>
                  {coupon.discountType === 'Percentage' 
                    ? `${coupon.discountValue}%` 
                    : `€${coupon.discountValue}`}
                </Table.Td>
                <Table.Td>
                  {coupon.minimumOrderAmount 
                    ? `€${coupon.minimumOrderAmount}` 
                    : '-'}
                </Table.Td>
                <Table.Td>
                  {new Date(coupon.endDate).toLocaleDateString()}
                </Table.Td>
                <Table.Td>
                  <Badge color={coupon.isActive ? 'green' : 'red'}>
                    {coupon.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </Table.Td>
                <Table.Td>
                  <Group gap="xs">
                    <ActionIcon
                      variant="subtle"
                      color="blue"
                      onClick={() => handleEdit(coupon)}
                    >
                      <IconEdit size={16} />
                    </ActionIcon>
                    <ActionIcon
                      variant="subtle"
                      color="red"
                      onClick={() => handleDelete(coupon.id)}
                    >
                      <IconTrash size={16} />
                    </ActionIcon>
                  </Group>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      <Modal
        opened={opened}
        onClose={close}
        title={modalMode === 'create' ? 'Add Coupon' : 'Edit Coupon'}
        size="lg"
      >
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <TextInput
              label="Coupon Code"
              placeholder="e.g. SUMMER2025"
              required
              {...form.getInputProps('code')}
            />

            <TextInput
              label="Description"
              placeholder="Describe this coupon..."
              {...form.getInputProps('description')}
            />

            <Select
              label="Discount Type"
              required
              data={[
                { value: 'Percentage', label: 'Percentage (%)' },
                { value: 'FixedAmount', label: 'Fixed Amount (€)' }
              ]}
              {...form.getInputProps('discountType')}
            />

            <NumberInput
              label="Discount Value (€)"
              required
              min={0}
              {...form.getInputProps('discountValue')}
            />

            <NumberInput
              label="Minimum Order Amount (€)"
              min={0}
              {...form.getInputProps('minimumOrderAmount')}
            />

            <DateInput
              label="Start Date"
              required
              {...form.getInputProps('startDate')}
            />

            <DateInput
              label="End Date"
              required
              {...form.getInputProps('endDate')}
            />

            <Switch
              label="Active"
              {...form.getInputProps('isActive', { type: 'checkbox' })}
            />

            <Group justify="flex-end" mt="md">
              <Button variant="outline" onClick={close}>Cancel</Button>
              <Button type="submit">Save</Button>
            </Group>
          </Stack>
        </form>
      </Modal>
    </div>
  );
};

export default Coupons;