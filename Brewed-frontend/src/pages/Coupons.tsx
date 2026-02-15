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
  Switch,
  MultiSelect,
  Checkbox,
  ScrollArea
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { DateInput } from "@mantine/dates";
import { IconEdit, IconTrash, IconPlus, IconRefresh, IconUsers } from "@tabler/icons-react";
import api from "../api/api";
import { ICoupon, IUserCoupon } from "../interfaces/ICoupon";
import { IUser } from "../interfaces/IUser";
import { notifications } from "@mantine/notifications";

const Coupons = () => {
  const [coupons, setCoupons] = useState<ICoupon[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedCoupon, setSelectedCoupon] = useState<ICoupon | null>(null);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [opened, { open, close }] = useDisclosure(false);
  const [usersModalOpened, { open: openUsersModal, close: closeUsersModal }] = useDisclosure(false);
  const [users, setUsers] = useState<IUser[]>([]);
  const [selectedCouponUsers, setSelectedCouponUsers] = useState<IUserCoupon[]>([]);

  const form = useForm({
    initialValues: {
      code: '',
      description: '',
      discountType: 'Percentage',
      discountValue: 0,
      minimumOrderAmount: 0,
      maxUsageCount: undefined as number | undefined,
      startDate: new Date(),
      endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000),
      isActive: true,
      generateRandomCode: false,
      userIds: [] as number[]
    },
    validate: {
      code: (value, values) => {
        if (values.generateRandomCode) return null;
        return !value ? 'Coupon code is required or enable random generation' : null;
      },
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

  const loadUsers = async () => {
    try {
      const response = await api.Users.getAllUsers();
      setUsers(response.data);
    } catch (error) {
      console.error("Failed to load users:", error);
    }
  };

  useEffect(() => {
    loadCoupons();
    loadUsers();
  }, []);

  const handleCreate = () => {
    setModalMode('create');
    form.reset();
    open();
  };

  const handleEdit = async (coupon: ICoupon) => {
    setModalMode('edit');
    setSelectedCoupon(coupon);

    // Load assigned users for this coupon
    try {
      const response = await api.Coupons.getCouponUsers(coupon.id);
      const assignedUserIds = response.data.map((uc: IUserCoupon) => uc.userId);

      form.setValues({
        code: coupon.code,
        description: coupon.description,
        discountType: coupon.discountType,
        discountValue: coupon.discountValue,
        minimumOrderAmount: coupon.minimumOrderAmount || 0,
        maxUsageCount: coupon.maxUsageCount,
        startDate: new Date(coupon.startDate),
        endDate: new Date(coupon.endDate),
        isActive: coupon.isActive,
        generateRandomCode: false,
        userIds: assignedUserIds
      });
    } catch (error) {
      console.error("Failed to load assigned users:", error);
      form.setValues({
        code: coupon.code,
        description: coupon.description,
        discountType: coupon.discountType,
        discountValue: coupon.discountValue,
        minimumOrderAmount: coupon.minimumOrderAmount || 0,
        maxUsageCount: coupon.maxUsageCount,
        startDate: new Date(coupon.startDate),
        endDate: new Date(coupon.endDate),
        isActive: coupon.isActive,
        generateRandomCode: false,
        userIds: []
      });
    }

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

  const handleGenerateCode = async () => {
    try {
      const response = await api.Coupons.generateRandomCode();
      form.setFieldValue('code', response.data.code);
      notifications.show({
        title: 'Success',
        message: 'Random code generated',
        color: 'green',
      });
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to generate random code',
        color: 'red',
      });
    }
  };

  const handleViewUsers = async (coupon: ICoupon) => {
    try {
      setLoading(true);
      setSelectedCoupon(coupon);
      const response = await api.Coupons.getCouponUsers(coupon.id);
      setSelectedCouponUsers(response.data);
      openUsersModal();
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to load coupon users',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: typeof form.values) => {
    try {
      setLoading(true);

      const couponData = {
        ...values,
        minimumOrderAmount: values.minimumOrderAmount || undefined,
        maxUsageCount: values.maxUsageCount || undefined
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
      console.error('Coupon save error:', error);

      let errorMessage = 'Failed to save coupon';

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

  return (
    <div style={{ position: 'relative' }}>
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
        <ScrollArea h="calc(100vh - 200px)">
          <Table striped highlightOnHover stickyHeader>
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
                        color="grape"
                        onClick={() => handleViewUsers(coupon)}
                        title="View assigned users"
                      >
                        <IconUsers size={16} />
                      </ActionIcon>
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
        </ScrollArea>
      )}

      <Modal
        opened={opened}
        onClose={close}
        title={modalMode === 'create' ? 'Add Coupon' : 'Edit Coupon'}
        size="lg"
      >
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            
            <Checkbox
              label="Generate Random Code"
              checked={form.values.generateRandomCode}
              onChange={async (event) => {
                const checked = event.currentTarget.checked;
                form.setFieldValue('generateRandomCode', checked);
                if (checked) {
                  try {
                    const response = await api.Coupons.generateRandomCode();
                    form.setFieldValue('code', response.data.code);
                  } catch {
                    notifications.show({
                      title: 'Error',
                      message: 'Failed to generate random code',
                      color: 'red',
                    });
                  }
                }
              }}
            />

            <Group grow>
              <TextInput
                label="Coupon Code"
                placeholder="e.g. SUMMER2025"
                required={!form.values.generateRandomCode}
                disabled={form.values.generateRandomCode}
                {...form.getInputProps('code')}
              />
              {/*!form.values.generateRandomCode && (
                <Button
                  variant="light"
                  onClick={handleGenerateCode}
                  mt="xl"
                  leftSection={<IconRefresh size={16} />}
                >
                  Generate
                </Button>
              )*/}
            </Group>

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
              label="Discount Value"
              required
              min={0}
              {...form.getInputProps('discountValue')}
            />

            <NumberInput
              label="Minimum Order Amount (€)"
              min={0}
              {...form.getInputProps('minimumOrderAmount')}
            />

            <NumberInput
              label="Maximum Usage Count (optional)"
              description="Leave empty for unlimited usage"
              min={1}
              {...form.getInputProps('maxUsageCount')}
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

            <MultiSelect
              label="Assign to Users"
              placeholder="Select users who can use this coupon"
              searchable
              data={users.map(user => ({
                value: user.id.toString(),
                label: `${user.name} (${user.email})`
              }))}
              value={form.values.userIds.map(id => id.toString())}
              onChange={(values) => form.setFieldValue('userIds', values.map(v => parseInt(v)))}
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

      <Modal
        opened={usersModalOpened}
        onClose={closeUsersModal}
        title={`Users for ${selectedCoupon?.code}`}
        size="lg"
      >
        {selectedCouponUsers.length === 0 ? (
          <Text ta="center" c="dimmed">No users assigned to this coupon</Text>
        ) : (
          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Name</Table.Th>
                <Table.Th>Email</Table.Th>
                <Table.Th>Status</Table.Th>
                <Table.Th>Assigned</Table.Th>
                <Table.Th>Used</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {selectedCouponUsers.map((userCoupon) => (
                <Table.Tr key={userCoupon.id}>
                  <Table.Td>{userCoupon.userName}</Table.Td>
                  <Table.Td>{userCoupon.userEmail}</Table.Td>
                  <Table.Td>
                    <Badge color={userCoupon.isUsed ? 'gray' : 'green'}>
                      {userCoupon.isUsed ? 'Used' : 'Available'}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    {new Date(userCoupon.assignedDate).toLocaleDateString()}
                  </Table.Td>
                  <Table.Td>
                    {userCoupon.usedDate
                      ? new Date(userCoupon.usedDate).toLocaleDateString()
                      : '-'}
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        )}
      </Modal>
    </div>
  );
};

export default Coupons;