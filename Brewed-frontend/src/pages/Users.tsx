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
  PasswordInput,
  ScrollArea,
  Tooltip
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { IconEdit, IconTrash, IconPlus, IconFileInvoice, IconStar } from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import { IUser } from "../interfaces/IUser";
import { notifications } from "@mantine/notifications";

interface UserFormValues {
  name: string;
  email: string;
  password?: string;
}

const Users = () => {
  const navigate = useNavigate();
  const [users, setUsers] = useState<IUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedUser, setSelectedUser] = useState<IUser | null>(null);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [opened, { open, close }] = useDisclosure(false);

  const form = useForm<UserFormValues>({
    initialValues: {
      name: '',
      email: '',
      password: ''
    },
    validate: {
      name: (value) => !value ? 'Name is required' : null,
      email: (value) => !/^\S+@\S+$/.test(value) ? 'Invalid email' : null,
      password: (value, values) => {
        if (modalMode === 'create' && (!value || value.length < 6)) {
          return 'Password must be at least 6 characters';
        }
        return null;
      }
    }
  });

  const loadUsers = async () => {
    try {
      setLoading(true);
      const response = await api.Users.getAllUsers();
      setUsers(response.data);
    } catch (error) {
      console.error("Failed to load users:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load users',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const handleCreate = () => {
    setModalMode('create');
    form.reset();
    open();
  };

  const handleEdit = (user: IUser) => {
    setModalMode('edit');
    setSelectedUser(user);
    form.setValues({
      name: user.name,
      email: user.email,
      password: ''
    });
    open();
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        setLoading(true);
        await api.Users.deleteUser(id);
        await loadUsers();
        notifications.show({
          title: 'Success',
          message: 'User deleted successfully',
          color: 'green',
        });
      } catch (error: any) {
        const errorMessage = error?.response?.data?.message || error?.message || 'Failed to delete user';
        notifications.show({
          title: 'Error',
          message: errorMessage,
          color: 'red',
        });
      } finally {
        setLoading(false);
      }
    }
  };

  const handleSubmit = async (values: UserFormValues) => {
    try {
      setLoading(true);

      if (modalMode === 'create') {
        await api.Users.createUser({
          name: values.name,
          email: values.email,
          password: values.password!
        });
        notifications.show({
          title: 'Success',
          message: 'User created successfully',
          color: 'green',
        });
      } else if (selectedUser) {
        await api.Users.updateUser(selectedUser.id, {
          name: values.name,
          email: values.email
        });
        notifications.show({
          title: 'Success',
          message: 'User updated successfully',
          color: 'green',
        });
      }

      await loadUsers();
      close();
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to save user',
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
        <Title order={2}>Users Management</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={handleCreate}>
          Add User
        </Button>
      </Group>

      {users.length === 0 ? (
        <Text ta="center" c="dimmed">No users found</Text>
      ) : (
        <ScrollArea h="calc(100vh - 200px)">
          <Table striped highlightOnHover stickyHeader>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Name</Table.Th>
                <Table.Th>Email</Table.Th>
                <Table.Th>Role</Table.Th>
                <Table.Th>Email Confirmed</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {users.map((user) => (
                <Table.Tr key={user.id}>
                  <Table.Td>
                    <Text fw={500}>{user.name}</Text>
                  </Table.Td>
                  <Table.Td>{user.email}</Table.Td>
                  <Table.Td>
                    <Badge color={user.role === 'Admin' ? 'red' : (user.role === 'Guest' ? 'orange' : 'blue')}>
                      {user.role}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={user.emailConfirmed ? 'green' : (user.role === 'Guest' ? 'orange' : 'gray')}>
                      {user.emailConfirmed ? 'Confirmed' : (user.role === 'Guest' ? 'Guest' : 'Pending')}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    <Group gap="xs">
                      <Tooltip label="View Orders">
                        <ActionIcon
                          variant="subtle"
                          color="green"
                          onClick={() => navigate(`/app/admin-orders?search=${encodeURIComponent(user.name)}`)}
                        >
                          <IconFileInvoice size={16} />
                        </ActionIcon>
                      </Tooltip>
                      <Tooltip label="View Reviews">
                        <ActionIcon
                          variant="subtle"
                          color="yellow"
                          onClick={() => navigate(`/app/admin-reviews?search=${encodeURIComponent(user.name)}`)}
                        >
                          <IconStar size={16} />
                        </ActionIcon>
                      </Tooltip>
                      <Tooltip label="Edit User">
                        <ActionIcon
                          variant="subtle"
                          color="blue"
                          onClick={() => handleEdit(user)}
                        >
                          <IconEdit size={16} />
                        </ActionIcon>
                      </Tooltip>
                      <Tooltip label="Delete User">
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={() => handleDelete(user.id)}
                        >
                          <IconTrash size={16} />
                        </ActionIcon>
                      </Tooltip>
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
        title={modalMode === 'create' ? 'Add User' : 'Edit User'}
        size="md"
      >
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <TextInput
              label="Name"
              placeholder="John Doe"
              required
              {...form.getInputProps('name')}
            />

            <TextInput
              label="Email"
              placeholder="john@example.com"
              required
              {...form.getInputProps('email')}
            />

            {modalMode === 'create' && (
              <PasswordInput
                label="Password"
                placeholder="Min 6 characters"
                required
                {...form.getInputProps('password')}
              />
            )}

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

export default Users;