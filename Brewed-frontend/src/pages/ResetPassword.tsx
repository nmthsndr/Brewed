import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { 
  Container, 
  Paper, 
  Title, 
  Text, 
  Button, 
  Stack, 
  PinInput, 
  Group, 
  Center,
  PasswordInput
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { IconCircleCheck, IconCircleX } from "@tabler/icons-react";
import { notifications } from "@mantine/notifications";
import api from "../api/api";

const ResetPassword = () => {
  const navigate = useNavigate();
  const [code, setCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState("");

  const passwordForm = useForm({
    initialValues: {
      newPassword: '',
      confirmPassword: ''
    },
    validate: {
      newPassword: (val) => {
        if (val.length < 8) return 'Password must be at least 8 characters';
        if (!/[A-Z]/.test(val)) return 'Password must contain at least one uppercase letter';
        if (!/[a-z]/.test(val)) return 'Password must contain at least one lowercase letter';
        if (!/[0-9]/.test(val)) return 'Password must contain at least one number';
        if (!/[^A-Za-z0-9]/.test(val)) return 'Password must contain at least one special character';
        return null;
      },
      confirmPassword: (val, values) => (val !== values.newPassword ? 'Passwords do not match' : null)
    }
  });

  const handleResetPassword = async (values: typeof passwordForm.values) => {
    if (code.length !== 6) {
      notifications.show({
        title: 'Error',
        message: 'Please enter the complete 6-digit code',
        color: 'red',
      });
      return;
    }

    try {
      setLoading(true);
      await api.Auth.resetPassword(code, values.newPassword);
      setSuccess(true);
      notifications.show({
        title: 'Success',
        message: 'Password reset successfully!',
        color: 'green',
      });
    } catch (err: any) {
      setError(err.response?.data?.Message || "Failed to reset password. The code may be expired or invalid.");
      notifications.show({
        title: 'Error',
        message: err.response?.data?.Message || 'Invalid verification code',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <Container size="sm" style={{ minHeight: "100vh", display: "flex", alignItems: "center" }}>
        <Paper 
          p="xl" 
          withBorder 
          style={{ 
            width: "100%",
            background: "linear-gradient(135deg, #D4A373 0%, #8B4513 100%)",
            padding: "2px",
            borderRadius: "12px"
          }}
        >
          <Paper p="xl" style={{ background: "white", borderRadius: "10px" }}>
            <Stack align="center" gap="lg">
              <IconCircleCheck size={80} color="#8B4513" />
              <Title order={2} style={{ color: "#8B4513" }}>Password Reset Complete!</Title>
              <Text c="dimmed" ta="center">
                Your password has been successfully reset. You can now log in with your new password.
              </Text>
              <Button 
                size="lg" 
                onClick={() => navigate("/login")}
                style={{
                  background: "linear-gradient(135deg, #D4A373 0%, #8B4513 100%)",
                  border: "none"
                }}
              >
                Go to Login
              </Button>
            </Stack>
          </Paper>
        </Paper>
      </Container>
    );
  }

  if (error) {
    return (
      <Container size="sm" style={{ minHeight: "100vh", display: "flex", alignItems: "center" }}>
        <Paper 
          p="xl" 
          withBorder 
          style={{ 
            width: "100%",
            background: "linear-gradient(135deg, #D4A373 0%, #8B4513 100%)",
            padding: "2px",
            borderRadius: "12px"
          }}
        >
          <Paper p="xl" style={{ background: "white", borderRadius: "10px" }}>
            <Stack align="center" gap="lg">
              <IconCircleX size={80} color="red" />
              <Title order={2} style={{ color: "#8B4513" }}>Reset Failed</Title>
              <Text c="red" ta="center">
                {error}
              </Text>
              <Group>
                <Button variant="outline" onClick={() => setError("")} color="brown">
                  Try Again
                </Button>
                <Button variant="outline" onClick={() => navigate("/forgot-password")} color="brown">
                  Request New Code
                </Button>
              </Group>
            </Stack>
          </Paper>
        </Paper>
      </Container>
    );
  }

  return (
    <Container size="sm" style={{ minHeight: "100vh", display: "flex", alignItems: "center" }}>
      <Paper 
        p="xl" 
        withBorder 
        style={{ 
          width: "100%",
          background: "linear-gradient(135deg, #D4A373 0%, #8B4513 100%)",
          padding: "2px",
          borderRadius: "12px"
        }}
      >
        <Paper p="xl" style={{ background: "white", borderRadius: "10px" }}>
          <Stack align="center" gap="lg">
            <Title order={2} style={{ color: "#8B4513" }}>Reset Your Password</Title>
            <Text c="dimmed" ta="center">
              Enter the 6-digit verification code sent to your email and your new password.
            </Text>
            
            <Center>
              <PinInput
                length={6}
                size="xl"
                type="number"
                value={code}
                onChange={setCode}
                placeholder="0"
                oneTimeCode
                styles={{
                  input: {
                    borderColor: "#D4A373",
                    '&:focus': {
                      borderColor: "#8B4513"
                    }
                  }
                }}
              />
            </Center>

            <form onSubmit={passwordForm.onSubmit(handleResetPassword)} style={{ width: '100%' }}>
              <Stack>
                <PasswordInput
                  label="New Password"
                  placeholder="Enter new password"
                  required
                  {...passwordForm.getInputProps('newPassword')}
                  styles={{
                    input: {
                      borderColor: "#D4A373",
                      '&:focus': {
                        borderColor: "#8B4513"
                      }
                    }
                  }}
                />

                <PasswordInput
                  label="Confirm New Password"
                  placeholder="Confirm new password"
                  required
                  {...passwordForm.getInputProps('confirmPassword')}
                  styles={{
                    input: {
                      borderColor: "#D4A373",
                      '&:focus': {
                        borderColor: "#8B4513"
                      }
                    }
                  }}
                />

                <Button 
                  type="submit"
                  size="lg" 
                  fullWidth 
                  loading={loading}
                  disabled={code.length !== 6}
                  style={{
                    background: code.length === 6 
                      ? "linear-gradient(135deg, #D4A373 0%, #8B4513 100%)" 
                      : "#ccc",
                    border: "none"
                  }}
                >
                  Reset Password
                </Button>
              </Stack>
            </form>

            <Text size="sm" c="dimmed" ta="center">
              Didn't receive the code? Check your spam folder or{" "}
              <Button 
                variant="subtle" 
                size="compact-sm" 
                onClick={() => navigate("/forgot-password")}
                color="brown"
              >
                request a new one
              </Button>
            </Text>
          </Stack>
        </Paper>
      </Paper>
    </Container>
  );
};

export default ResetPassword;