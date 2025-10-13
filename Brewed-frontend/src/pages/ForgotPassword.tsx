import { TextInput, Button, Group, Stack, Text } from "@mantine/core";
import { useForm } from "@mantine/form";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import AuthContainer from "../components/AuthContainer";
import { notifications } from "@mantine/notifications";
import api from "../api/api";

const ForgotPassword = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const form = useForm({
    initialValues: {
      email: '',
    },
    validate: {
      email: (val: string) => (/^\S+@\S+$/.test(val) ? null : 'Invalid email address'),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setLoading(true);
    try {
      await api.Auth.forgotPassword(values.email);
      notifications.show({
        title: 'Email Sent',
        message: 'If your email is in our system, you will receive password reset instructions.',
        color: 'blue',
      });
      form.reset();
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to send reset email. Please try again.',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthContainer>
      <div>
        <Text mb="md">
          Enter your email address and we'll send you instructions to reset your password.
        </Text>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <TextInput
              required
              label="Email Address"
              placeholder="your.email@example.com"
              {...form.getInputProps('email')}
            />
          </Stack>
          <Group justify="space-between" mt="xl">
            <Button variant="outline" onClick={() => navigate('/login')}>
              Back to Login
            </Button>
            <Button type="submit" loading={loading}>
              Send Reset Link
            </Button>
          </Group>
        </form>
      </div>
    </AuthContainer>
  );
};

export default ForgotPassword;