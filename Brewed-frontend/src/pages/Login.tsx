import {
  Stack,
  TextInput,
  PasswordInput,
  Group,
  Button,
  Anchor,
  Divider
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { notifications } from "@mantine/notifications";
import AuthContainer from "../components/AuthContainer";
import useAuth from "../hooks/useAuth";

const Login = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const form = useForm({
    initialValues: {
      email: '',
      password: '',
    },
    validate: {
      email: (val: string) => (/^\S+@\S+$/.test(val) ? null : 'Invalid email address'),
      password: (val: string) => (val.length < 6 ? 'Password must be at least 6 characters' : null),
    },
  });

  const submit = async () => {
    setLoading(true);
    try {
      const success = await login(form.values.email, form.values.password);
      if (success) {
        notifications.show({
          title: 'Success',
          message: 'Logged in successfully',
          color: 'green',
        });
        navigate('/app/dashboard');
      } else {
        notifications.show({
          title: 'Error',
          message: 'Invalid email or password',
          color: 'red',
        });
      }
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Login failed. Please try again.',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthContainer>
      <div>
        <form onSubmit={form.onSubmit(submit)}>
          <Stack>
            <TextInput
              required
              label="Email"
              placeholder="your.email@example.com"
              radius="md"
              {...form.getInputProps('email')}
              styles={{
                input: {
                  borderColor: '#D4A373',
                  '&:focus': {
                    borderColor: '#8B4513'
                  }
                }
              }}
            />

            <PasswordInput
              required
              label="Password"
              placeholder="Your password"
              radius="md"
              {...form.getInputProps('password')}
              styles={{
                input: {
                  borderColor: '#D4A373',
                  '&:focus': {
                    borderColor: '#8B4513'
                  }
                }
              }}
            />
          </Stack>

          <Group justify="space-between" mt="xl">
            <Anchor
              component="button"
              type="button"
              c="#8B4513"
              onClick={() => navigate('/forgot-password')}
              size="xs"
            >
              Forgot your password?
            </Anchor>
            <Button 
              type="submit" 
              radius="xl" 
              loading={loading}
              style={{
                background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
                border: 'none'
              }}
            >
              Login
            </Button>
          </Group>

          <Divider my="lg" />

          <Group justify="center">
            <Anchor
              component="button"
              type="button"
              onClick={() => navigate('/register')}
              size="sm"
              c="#8B4513"
            >
              Don't have an account? Register
            </Anchor>
          </Group>

          <Group justify="center" mt="md">
            <Anchor
              component="button"
              type="button"
              onClick={() => navigate('/app/dashboard')}
              size="sm"
              c="dimmed"
            >
              ← Back to Dashboard
            </Anchor>
          </Group>
        </form>
      </div>
    </AuthContainer>
  );
};

export default Login;