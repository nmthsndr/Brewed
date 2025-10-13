import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Container, Paper, Title, Text, Button, Stack, PinInput, Group } from "@mantine/core";
import { IconCircleCheck, IconCircleX } from "@tabler/icons-react";
import { notifications } from "@mantine/notifications";
import api from "../api/api";

const ConfirmEmail = () => {
  const navigate = useNavigate();
  const [code, setCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState("");

  const handleConfirm = async () => {
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
      await api.Auth.confirmEmail(code);
      setSuccess(true);
      notifications.show({
        title: 'Success',
        message: 'Email confirmed successfully!',
        color: 'green',
      });
    } catch (err: any) {
      setError(err.response?.data?.Message || "Failed to confirm email. The code may be expired or invalid.");
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
        <Paper p="xl" withBorder style={{ width: "100%" }}>
          <Stack align="center" gap="lg">
            <IconCircleCheck size={80} color="green" />
            <Title order={2}>Email Confirmed!</Title>
            <Text c="dimmed" ta="center">
              Your email has been successfully confirmed. You can now log in to your account.
            </Text>
            <Button size="lg" onClick={() => navigate("/login")}>
              Go to Login
            </Button>
          </Stack>
        </Paper>
      </Container>
    );
  }

  if (error) {
    return (
      <Container size="sm" style={{ minHeight: "100vh", display: "flex", alignItems: "center" }}>
        <Paper p="xl" withBorder style={{ width: "100%" }}>
          <Stack align="center" gap="lg">
            <IconCircleX size={80} color="red" />
            <Title order={2}>Confirmation Failed</Title>
            <Text c="red" ta="center">
              {error}
            </Text>
            <Group>
              <Button variant="outline" onClick={() => setError("")}>
                Try Again
              </Button>
              <Button variant="outline" onClick={() => navigate("/register")}>
                Back to Register
              </Button>
            </Group>
          </Stack>
        </Paper>
      </Container>
    );
  }

  return (
    <Container size="sm" style={{ minHeight: "100vh", display: "flex", alignItems: "center" }}>
      <Paper p="xl" withBorder style={{ width: "100%" }}>
        <Stack align="center" gap="lg">
          <Title order={2}>Verify Your Email</Title>
          <Text c="dimmed" ta="center">
            Please enter the 6-digit verification code sent to your email address.
          </Text>
          
          <PinInput
            length={6}
            size="xl"
            type="number"
            value={code}
            onChange={setCode}
            placeholder="0"
            oneTimeCode
          />

          <Button 
            size="lg" 
            fullWidth 
            onClick={handleConfirm}
            loading={loading}
            disabled={code.length !== 6}
          >
            Verify Email
          </Button>

          <Text size="sm" c="dimmed" ta="center">
            Didn't receive the code? Check your spam folder or{" "}
            <Button variant="subtle" size="compact-sm" onClick={() => navigate("/register")}>
              register again
            </Button>
          </Text>
        </Stack>
      </Paper>
    </Container>
  );
};

export default ConfirmEmail;