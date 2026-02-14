import { Center, Divider, Image, Paper, Text } from "@mantine/core";

interface AuthContainerProps {
  children: JSX.Element;
}

const AuthContainer = ({ children }: AuthContainerProps) => {
  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(160deg, #faf8f5 0%, #f0e8dd 40%, #e8ddd0 100%)',
      paddingTop: '60px',
      paddingBottom: '40px',
    }}>
      <Center>
        <Image
          src="/logo.png"
          alt="Brewed Logo"
          w={100}
          h={100}
          mb={24}
          style={{
            borderRadius: '50%',
            objectFit: 'cover',
            border: '3px solid rgba(139, 69, 19, 0.15)',
            boxShadow: '0 8px 32px rgba(139, 69, 19, 0.12)',
          }}
        />
      </Center>
      <Center>
        <Paper
          radius="xl"
          p="xl"
          withBorder
          maw={460}
          w="100%"
          m={10}
          style={{
            borderColor: 'rgba(139, 69, 19, 0.1)',
            boxShadow: '0 8px 40px rgba(139, 69, 19, 0.08), 0 2px 8px rgba(0, 0, 0, 0.04)',
            background: 'rgba(255, 255, 255, 0.95)',
            backdropFilter: 'blur(12px)',
          }}
        >
          <Text
            size="xl"
            fw={700}
            ta="center"
            mb="xs"
            style={{
              fontFamily: '"Playfair Display", Georgia, serif',
              color: '#8B4513',
            }}
          >
            Welcome to Brewed
          </Text>
          <Text size="sm" c="dimmed" ta="center" mb="md">
            Your premium coffee experience
          </Text>
          <Divider my="lg" color="rgba(139, 69, 19, 0.1)" />
          {children}
        </Paper>
      </Center>
    </div>
  );
};

export default AuthContainer;