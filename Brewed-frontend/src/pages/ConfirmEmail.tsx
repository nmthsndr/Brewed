import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Container, Paper, Title, Text, Loader, Button, Stack } from "@mantine/core";
import { IconCircleCheck, IconCircleX } from "@tabler/icons-react";
import api from "../api/api";

const ConfirmEmail = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    const confirmEmail = async () => {
      const token = searchParams.get("token");

      if (!token) {
        setError("Invalid confirmation link");
        setLoading(false);
        return;
      }

      try {
        await api.Auth.confirmEmail(token);
        setSuccess(true);
      } catch (err: any) {
        setError(err.response?.data || "Failed to confirm email. The link may be expired or invalid.");
      } finally {
        setLoading(false);
      }
    };

    confirmEmail();
  }, [searchParams]);

  return (
    <Container size="sm" style={{ minHeight: "100vh", display: "flex", alignItems: "center" }}>
      <Paper p="xl" withBorder style={{ width: "100%" }}>
        <Stack align="center" gap="lg">
          {loading ? (
            <>
              <Loader size="xl" />
              <Title order={3}>Confirming your email...</Title>
              <Text c="dimmed">Please wait a moment</Text>
            </>
          ) : success ? (
            <>
              <IconCircleCheck size={80} color="green" />
              <Title order={2}>Email Confirmed!</Title>
              <Text c="dimmed" ta="center">
                Your email has been successfully confirmed. You can now log in to your account.
              </Text>
              <Button size="lg" onClick={() => navigate("/login")}>
                Go to Login
              </Button>
            </>
          ) : (
            <>
              <IconCircleX size={80} color="red" />
              <Title order={2}>Confirmation Failed</Title>
              <Text c="red" ta="center">
                {error}
              </Text>
              <Button variant="outline" onClick={() => navigate("/register")}>
                Back to Register
              </Button>
            </>
          )}
        </Stack>
      </Paper>
    </Container>
  );
};

export default ConfirmEmail;