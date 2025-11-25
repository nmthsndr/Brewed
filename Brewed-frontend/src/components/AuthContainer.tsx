import { Center, Divider, Image, Paper, Text } from "@mantine/core";

interface AuthContainerProps {
  children: JSX.Element;
}

const AuthContainer = ({ children }: AuthContainerProps) => {
  return (
    <div style={{ minHeight: '100vh', background: '#f5f5f5', paddingTop: '50px' }}>
      <Center>
        <Image
          src="/logo.png"
          alt="Brewed Logo"
          w={150}
          mb={20}
          style={{
            borderRadius: '50%',
            objectFit: 'cover',
            //border:'1px solid black'
          }}
        />
      </Center>
      <Center>
        <Paper radius="md" p="xl" withBorder maw={500} w="100%" m={10}>
          <Text size="lg" fw={500} ta="center" mb="md">
            Welcome to Brewed
          </Text>
          <Divider my="lg" />
          {children}
        </Paper>
      </Center>
    </div>
  );
};

export default AuthContainer;