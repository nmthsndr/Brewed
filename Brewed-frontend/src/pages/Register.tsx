import {
  Stack,
  TextInput,
  PasswordInput,
  Group,
  Button,
  Anchor,
  Divider,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { notifications } from "@mantine/notifications";
import AuthContainer from "../components/AuthContainer";
import api from "../api/api";

const Register = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const form = useForm({
    initialValues: {
      name: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
    validate: {
      name: (val: string) => (val.length < 3 ? 'Name must be at least 3 characters' : null),
      email: (val: string) => (/^\S+@\S+$/.test(val) ? null : 'Invalid email address'),
      password: (val: string) => (val.length < 6 ? 'Password must be at least 6 characters' : null),
      confirmPassword: (val: string, values) => (val !== values.password ? 'Passwords do not match' : null),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setLoading(true);
    try {
      await api.Auth.register({
        name: values.name,
        email: values.email,
        password: values.password,
      });

      notifications.show({
        title: 'Success',
        message: 'Registration successful! Please check your email to confirm your account.',
        color: 'green',
      });

      navigate('/login');
    } catch (error: any) {
      notifications.show({
        title: 'Registration Failed',
        message: error.response?.data?.message || 'Could not create account. Please try again.',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthContainer>
      <div>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <TextInput
              required
              label="Name"
              placeholder="John Doe"
              radius="md"
              {...form.getInputProps('name')}
            />

            <TextInput
              required
              label="Email"
              placeholder="your.email@example.com"
              radius="md"
              {...form.getInputProps('email')}
            />

            <PasswordInput
              required
              label="Password"
              placeholder="Your password"
              radius="md"
              {...form.getInputProps('password')}
            />

            <PasswordInput
              required
              label="Confirm Password"
              placeholder="Confirm your password"
              radius="md"
              {...form.getInputProps('confirmPassword')}
            />
          </Stack>

          <Group justify="space-between" mt="xl">
            <Anchor
              component="button"
              type="button"
              c="dimmed"
              onClick={() => navigate('/login')}
              size="xs"
            >
              Already have an account? Login
            </Anchor>
            <Button type="submit" radius="xl" loading={loading}>
              Register
            </Button>
          </Group>
        </form>
      </div>
    </AuthContainer>
  );
};

export default Register;