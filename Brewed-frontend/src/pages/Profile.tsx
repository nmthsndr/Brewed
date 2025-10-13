import { useState, useEffect } from "react";
import {
  Title,
  Tabs,
  Paper,
  TextInput,
  Button,
  Group,
  Stack,
  LoadingOverlay,
  PasswordInput
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { IconUser, IconMapPin, IconLock } from "@tabler/icons-react";
import api from "../api/api";
import AddressManagement from "../components/AddressManagement";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";

const Profile = () => {
  const [loading, setLoading] = useState(false);
  const { email } = useAuth();

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

  return (
    <div style={{ padding: '20px', position: 'relative' }}>
      <LoadingOverlay visible={loading} />
      <Title order={2} mb="lg">My Profile</Title>

      <Tabs defaultValue="profile">
        <Tabs.List>
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

        <Tabs.Panel value="profile" pt="md">
          <Paper withBorder p="lg" maw={600}>
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
    </div>
  );
};

export default Profile;