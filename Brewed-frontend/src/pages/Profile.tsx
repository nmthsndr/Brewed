import { useState, useEffect } from "react";
import {
  Title,
  Text,
  Tabs,
  Paper,
  TextInput,
  Button,
  Group,
  Stack,
  LoadingOverlay,
  PasswordInput,
  Modal
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { IconUser, IconMapPin, IconLock, IconTrash } from "@tabler/icons-react";
import { useSearchParams, useNavigate } from "react-router-dom";
import api from "../api/api";
import AddressManagement from "../components/AddressManagement";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";

const Profile = () => {
  const [loading, setLoading] = useState(false);
  const { email, logout } = useAuth();
  const [searchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState(searchParams.get('tab') || 'profile');
  const [deleteModalOpened, { open: openDeleteModal, close: closeDeleteModal }] = useDisclosure(false);
  const navigate = useNavigate();

  const profileForm = useForm({
    initialValues: {
      name: '',
      email: ''
    },
    validate: {
      name: (val) => (val.length < 3 ? 'Name must be at least 3 characters' : null),
      email: (val) => (/^\S+@\S+$/.test(val) ? null : 'Invalid email')
    }
  });

  const passwordForm = useForm({
    initialValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    },
    validate: {
      currentPassword: (val) => (val.length < 6 ? 'Password must be at least 6 characters' : null),
      newPassword: (val) => (val.length < 6 ? 'Password must be at least 6 characters' : null),
      confirmPassword: (val, values) => (val !== values.newPassword ? 'Passwords do not match' : null)
    }
  });

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      const response = await api.Users.getProfile();
      profileForm.setValues({
        name: response.data.name,
        email: response.data.email
      });
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to load profile',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateProfile = async (values: typeof profileForm.values) => {
    try {
      setLoading(true);
      await api.Users.updateProfile(values);
      notifications.show({
        title: 'Success',
        message: 'Profile updated successfully',
        color: 'green',
      });
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to update profile',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleChangePassword = async (values: typeof passwordForm.values) => {
    try {
      setLoading(true);
      await api.Auth.changePassword(values.currentPassword, values.newPassword);
      notifications.show({
        title: 'Success',
        message: 'Password changed successfully',
        color: 'green',
      });
      passwordForm.reset();
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to change password',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteAccount = async () => {
    try {
      setLoading(true);
      await api.Users.deleteProfile();
      notifications.show({
        title: 'Account Deleted',
        message: 'Your account has been deleted successfully.',
        color: 'green',
      });
      closeDeleteModal();
      logout();
      navigate('/login');
    } catch (error: any) {
      const errorMessage = error?.response?.data?.message || 'Failed to delete account';
      notifications.show({
        title: 'Error',
        message: errorMessage,
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const tab = searchParams.get('tab');
    if (tab) {
      setActiveTab(tab);
    }
  }, [searchParams]);

  return (
    <div style={{ position: 'relative' }}>
      <LoadingOverlay visible={loading} />
      <Title order={2} mb="xs" style={{ color: '#3d3d3d' }}>My Profile</Title>
      <Text size="sm" c="dimmed" mb="lg">Manage your account settings</Text>

      <Tabs value={activeTab} onChange={(value) => setActiveTab(value || 'profile')} color="brown">
        <Tabs.List style={{ borderBottomColor: 'rgba(139, 69, 19, 0.1)' }}>
          <Tabs.Tab value="profile" leftSection={<IconUser size={16} />}>
            Profile Information
          </Tabs.Tab>
          <Tabs.Tab value="addresses" leftSection={<IconMapPin size={16} />}>
            Addresses
          </Tabs.Tab>
          <Tabs.Tab value="password" leftSection={<IconLock size={16} />}>
            Change Password
          </Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="profile" pt="lg">
          <Paper withBorder p="lg" maw={600} style={{ borderColor: 'rgba(139, 69, 19, 0.1)' }}>
            <form onSubmit={profileForm.onSubmit(handleUpdateProfile)}>
              <Stack>
                <TextInput
                  label="Name"
                  placeholder="Your name"
                  required
                  {...profileForm.getInputProps('name')}
                />
                <TextInput
                  label="Email"
                  placeholder="your.email@example.com"
                  required
                  {...profileForm.getInputProps('email')}
                />
                <Group justify="flex-end" mt="md">
                  <Button type="submit">Save Changes</Button>
                </Group>
              </Stack>
            </form>
          </Paper>

          <Paper withBorder p="lg" maw={600} mt="xl" style={{ borderColor: '#e03131' }}>
            <Title order={4} c="red" mb="sm">Delete Account</Title>
            <Text size="sm" c="dimmed" mb="md">
              Once your account is deleted, your profile, orders, and reviews will no longer be visible. This action cannot be undone.
            </Text>
            <Button
              color="red"
              variant="outline"
              leftSection={<IconTrash size={16} />}
              onClick={openDeleteModal}
            >
              Delete My Account
            </Button>
          </Paper>
        </Tabs.Panel>

        <Tabs.Panel value="addresses" pt="md">
          <AddressManagement />
        </Tabs.Panel>

        <Tabs.Panel value="password" pt="md">
          <Paper withBorder p="lg" maw={600}>
            <form onSubmit={passwordForm.onSubmit(handleChangePassword)}>
              <Stack>
                <PasswordInput
                  label="Current Password"
                  placeholder="Current password"
                  required
                  {...passwordForm.getInputProps('currentPassword')}
                />
                <PasswordInput
                  label="New Password"
                  placeholder="New password"
                  required
                  {...passwordForm.getInputProps('newPassword')}
                />
                <PasswordInput
                  label="Confirm New Password"
                  placeholder="Confirm new password"
                  required
                  {...passwordForm.getInputProps('confirmPassword')}
                />
                <Group justify="flex-end" mt="md">
                  <Button type="submit">Change Password</Button>
                </Group>
              </Stack>
            </form>
          </Paper>
        </Tabs.Panel>
      </Tabs>

      <Modal
        opened={deleteModalOpened}
        onClose={closeDeleteModal}
        title="Delete Account"
        centered
      >
        <Text mb="lg">
          Are you sure you want to delete your account? This action cannot be undone. Your profile, orders, and reviews will no longer be visible.
        </Text>
        <Group justify="flex-end">
          <Button variant="outline" onClick={closeDeleteModal}>Cancel</Button>
          <Button color="red" onClick={handleDeleteAccount}>Delete Account</Button>
        </Group>
      </Modal>
    </div>
  );
};

export default Profile;
