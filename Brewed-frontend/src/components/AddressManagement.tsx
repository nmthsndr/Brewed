import { useEffect, useState } from "react";
import {
  Table,
  Group,
  Button,
  ActionIcon,
  Text,
  Modal,
  TextInput,
  Stack,
  LoadingOverlay,
  Checkbox
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { IconEdit, IconTrash, IconPlus, IconStar } from "@tabler/icons-react";
import api, { AddressCreateDto } from "../api/api";
import { IAddress } from "../interfaces/IAddress";
import { notifications } from "@mantine/notifications";

const AddressManagement = () => {
  const [addresses, setAddresses] = useState<IAddress[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedAddress, setSelectedAddress] = useState<IAddress | null>(null);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [opened, { open, close }] = useDisclosure(false);

  const form = useForm<AddressCreateDto>({
    initialValues: {
      firstName: '',
      lastName: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      postalCode: '',
      country: '',
      phoneNumber: '',
      isDefault: false,
      addressType: 'Shipping'
    },
    validate: {
      firstName: (value) => !value ? 'First name is required' : null,
      lastName: (value) => !value ? 'Last name is required' : null,
      addressLine1: (value) => !value ? 'Address is required' : null,
      city: (value) => !value ? 'City is required' : null,
      postalCode: (value) => !value ? 'Postal code is required' : null,
      country: (value) => !value ? 'Country is required' : null,
      phoneNumber: (value) => !value ? 'Phone number is required' : null
    }
  });

  const loadAddresses = async () => {
    try {
      setLoading(true);
      const response = await api.Addresses.getAddresses();
      setAddresses(response.data);
    } catch (error) {
      console.error("Failed to load addresses:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load addresses',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAddresses();
  }, []);

  const handleCreate = () => {
    setModalMode('create');
    form.reset();
    open();
  };

  const handleEdit = (address: IAddress) => {
    setModalMode('edit');
    setSelectedAddress(address);
    form.setValues(address);
    open();
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this address?')) {
      try {
        setLoading(true);
        await api.Addresses.deleteAddress(id);
        await loadAddresses();
        notifications.show({
          title: 'Success',
          message: 'Address deleted successfully',
          color: 'green',
        });
      } catch (error) {
        notifications.show({
          title: 'Error',
          message: 'Failed to delete address',
          color: 'red',
        });
        } finally {
        setLoading(false);
      }
    }
  };
  const handleSetDefault = async (id: number) => {
    try {
      setLoading(true);
      await api.Addresses.setDefaultAddress(id);
      await loadAddresses();
      notifications.show({
        title: 'Success',
        message: 'Default address updated',
        color: 'green',
      });
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to update default address',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };
  const handleSubmit = async (values: AddressCreateDto) => {
    try {
      setLoading(true);

      if (modalMode === 'create') {
        await api.Addresses.createAddress(values);
        notifications.show({
          title: 'Success',
          message: 'Address created successfully',
          color: 'green',
        });
      } else if (selectedAddress) {
        await api.Addresses.updateAddress(selectedAddress.id, values);
        notifications.show({
          title: 'Success',
          message: 'Address updated successfully',
          color: 'green',
        });
      }

      await loadAddresses();
      close();
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to save address',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };
  return (
    <div style={{ position: 'relative' }}>
      <LoadingOverlay visible={loading} />

      <Group justify="space-between" mb="md">
        <Button leftSection={<IconPlus size={16} />} onClick={handleCreate}>
          Add Address
        </Button>
      </Group>

      {addresses.length === 0 ? (
        <Text ta="center" c="dimmed">No addresses found</Text>
      ) : (
        <Table striped highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Name</Table.Th>
              <Table.Th>Address</Table.Th>
              <Table.Th>City</Table.Th>
              <Table.Th>Phone</Table.Th>
              <Table.Th>Type</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {addresses.map((address) => (
              <Table.Tr key={address.id}>
                <Table.Td>
                  <Group gap="xs">
                    {address.firstName} {address.lastName}
                    {address.isDefault && <IconStar size={16} fill="gold" color="gold" />}
                  </Group>
                </Table.Td>
                <Table.Td>{address.addressLine1}</Table.Td>
                <Table.Td>{address.city}, {address.postalCode}</Table.Td>
                <Table.Td>{address.phoneNumber}</Table.Td>
                <Table.Td>{address.addressType}</Table.Td>
                <Table.Td>
                  <Group gap="xs">
                    {!address.isDefault && (
                      <ActionIcon
                        variant="subtle"
                        color="yellow"
                        onClick={() => handleSetDefault(address.id)}
                        title="Set as default"
                      >
                        <IconStar size={16} />
                      </ActionIcon>
                    )}
                    <ActionIcon
                      variant="subtle"
                      color="blue"
                      onClick={() => handleEdit(address)}
                    >
                      <IconEdit size={16} />
                    </ActionIcon>
                    <ActionIcon
                      variant="subtle"
                      color="red"
                      onClick={() => handleDelete(address.id)}
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
        title={modalMode === 'create' ? 'Add Address' : 'Edit Address'}
        size="lg"
      >
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <Group grow>
              <TextInput
                label="First Name"
                placeholder="John"
                required
                {...form.getInputProps('firstName')}
              />
              <TextInput
                label="Last Name"
                placeholder="Doe"
                required
                {...form.getInputProps('lastName')}
              />
            </Group>

            <TextInput
              label="Address Line 1"
              placeholder="123 Main St"
              required
              {...form.getInputProps('addressLine1')}
            />

            <TextInput
              label="Address Line 2"
              placeholder="Apt 4B"
              {...form.getInputProps('addressLine2')}
            />

            <Group grow>
              <TextInput
                label="City"
                placeholder="New York"
                required
                {...form.getInputProps('city')}
              />
              <TextInput
                label="Postal Code"
                placeholder="10001"
                required
                {...form.getInputProps('postalCode')}
              />
            </Group>

            <TextInput
              label="Country"
              placeholder="USA"
              required
              {...form.getInputProps('country')}
            />

            <TextInput
              label="Phone Number"
              placeholder="+1 (555) 123-4567"
              required
              {...form.getInputProps('phoneNumber')}
            />

            <Checkbox
              label="Set as default address"
              {...form.getInputProps('isDefault', { type: 'checkbox' })}
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

export default AddressManagement;