import Vue from "vue";
import VueRouter, { RouteConfig, Route } from "vue-router";
import About from "@/views/About.vue";
import Home from "@/views/Home.vue";
import Play from "@/views/Play.vue";
import Signin from "@/views/Signin.vue";
import Signup from "@/views/Signup.vue";
import Account from "@/views/Account.vue";
import { AuthModule } from "@/store";

Vue.use(VueRouter);

const routes: RouteConfig[] = [
  {
    path: "/signin",
    name: "signin",
    component: Signin,
    meta: {
      anonymous: true,
    },
  },
  {
    path: "/signup/:code",
    name: "signup",
    component: Signup,
    meta: {
      anonymous: true,
    },
  },
  {
    path: "/account",
    name: "account",
    component: Account,
  },
  {
    path: "/about",
    name: "about",
    component: About,
  },
  {
    path: "/play/",
    name: "play",
    component: Play,
    props: (route: Route) => ({
      path: route.query.path,
    }),
  },
  {
    path: "/:request*",
    name: "home",
    component: Home,
    props: (route: Route) => ({
      path: route.params.request,
    }),
  },
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes,
});

router.beforeEach(async (to, _, next) => {
  if (to.meta.anonymous) {
    if (await AuthModule.isSignedIn()) {
      next({ name: "home" });
    } else {
      next();
    }
  } else {
    if (await AuthModule.isSignedIn()) {
      next();
    } else {
      next({ name: "signin" });
    }
  }
});

export default router;
