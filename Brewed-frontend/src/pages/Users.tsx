import { Title, Text } from "@mantine/core";

const Users = () => {
  return (
    <div style={{ padding: '20px' }}>
      <Title order={2} mb="lg">Users Management</Title>
      <Text c="dimmed">User management coming soon...</Text>
      <Text c="dimmed" mt="md">
        Note: User management requires additional backend endpoints for listing and managing users.
      </Text>
    </div>
  );
};

export default Users;