import { ActionIcon, Group, Image, Text } from "@mantine/core";
import {
  IconBrandInstagram,
  IconBrandTwitter,
  IconBrandFacebook,
} from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import classes from "./Footer.module.css";

const links = [
  { label: "Products", url: "/app/products" },
  { label: "Cart", url: "/app/cart" },
  { label: "Dashboard", url: "/app/dashboard" },
];

const Footer = () => {
  const navigate = useNavigate();

  return (
    <div className={classes.inner}>
      <div></div>
      <Text
          fw={500}
          size="xl"
          style={{
            fontFamily: 'serif',
            color: '#8B4513',
            letterSpacing: '-0.02em',
          }}
          visibleFrom="sm"
        >
          © 2026 Brewed 
        </Text>

      <Group gap="xs" wrap="nowrap" className={classes.socials}>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Twitter" href="https://x.com" component="a" target="_blank">
          <IconBrandTwitter size={18} stroke={1.5} />
        </ActionIcon>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Facebook" href="https://facebook.com" component="a" target="_blank">
          <IconBrandFacebook size={18} stroke={1.5} />
        </ActionIcon>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Instagram" href="https://instagram.com" component="a" target="_blank">
          <IconBrandInstagram size={18} stroke={1.5} />
        </ActionIcon>
      </Group>
    </div>
  );
};

export default Footer;