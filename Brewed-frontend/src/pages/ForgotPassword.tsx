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
  const [emailSent, setEmailSent] = useState(false);

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
      setEmailSent(true);
      notifications.show({
        title: 'Email Sent',
        message: 'If your email is in our system, you will receive a 6-digit verification code.',
        color: 'blue',
      });
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

  if (emailSent) {
    return (
      <AuthContainer>
        <div>
          <Text size="lg" fw={500} ta="center" mb="md">
            Check Your Email
          </Text>
          <Text mb="md" ta="center">
            We've sent a 6-digit verification code to your email address.
          </Text>
          <Text mb="lg" ta="center" c="dimmed" size="sm">
            Please check your inbox and spam folder.
          </Text>
          <Button 
            fullWidth 
            onClick={() => navigate('/reset-password')}
            style={{
              background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
              border: 'none'
            }}
          >
            Enter Verification Code
          </Button>
          <Group justify="center" mt="md">
            <Button 
              variant="subtle" 
              onClick={() => setEmailSent(false)}
              color="brown"
            >
              Resend Code
            </Button>
          </Group>
        </div>
      </AuthContainer>
    );
  }

  return (
    <AuthContainer>
      <div>
        <Text mb="md">
          Enter your email address and we'll send you a 6-digit verification code to reset your password.
        </Text>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <TextInput
              required
              label="Email Address"
              placeholder="your.email@example.com"
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
          </Stack>
          <Group justify="space-between" mt="xl">
            <Button 
              variant="outline" 
              onClick={() => navigate('/login')}
              color="brown"
            >
              Back to Login
            </Button>
            <Button 
              type="submit" 
              loading={loading}
              style={{
                background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
                border: 'none'
              }}
            >
              Send Code
            </Button>
          </Group>
        </form>
      </div>
    </AuthContainer>
  );
};

export default ForgotPassword;