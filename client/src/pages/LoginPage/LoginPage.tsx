import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "@/contexts";
import { Button, Input, Label, Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle, Separator } from "@/components/ui";
import { Cloud, Mail, Lock, Eye, EyeOff, X } from "lucide-react";
//import cloudHero from "@/assets/cloud-hero.jpg";

const LoginPage = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [showPassword, setShowPassword] = useState(false);
  const [showForgot, setShowForgot] = useState(false);
  const [forgotEmail, setForgotEmail] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    email: "",
    password: "",
    confirmPassword: ""
  });

  const { login, isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  type LocationState = { from?: { pathname?: string } } | null;
  const from = ((location.state as LocationState)?.from?.pathname) || "/";

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/", { replace: true });
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!isLogin) {
      // Registration would call a separate endpoint; omitted for brevity
      setError("Sign up is not implemented yet.");
      return;
    }
    try {
      await login(formData.email, formData.password);
      navigate(from, { replace: true });
    } catch (err: unknown) {
      let message = "Failed to sign in";
      if (err && typeof err === "object") {
        const maybeBody = (err as { body?: unknown }).body;
        if (maybeBody && typeof maybeBody === "object" && "message" in (maybeBody as Record<string, unknown>)) {
          const m = (maybeBody as Record<string, unknown>).message;
          if (typeof m === "string" && m.trim()) message = m;
        } else if ("message" in (err as Record<string, unknown>)) {
          const m = (err as Record<string, unknown>).message;
          if (typeof m === "string" && m.trim()) message = m;
        }
      }
      setError(message);
    }
  };

  const handleForgotSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log("ForgotPassword", { email: forgotEmail });
    setShowForgot(false);
    setForgotEmail("");
  };

  return (
    <div className="h-screen w-screen flex overflow-hidden">
      {/* Left side - Hero Image */}
      <div className="hidden lg:flex lg:w-1/2 h-full relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-hero"></div>
        <div className="absolute inset-0 flex items-center justify-center text-white p-12">
          <div className="text-center">
            <div className="flex items-center justify-center mb-6">
              <Cloud className="h-16 w-16 mr-4" />
              <h1 className="text-5xl font-bold">Cloudy</h1>
            </div>
            <p className="text-xl opacity-90 max-w-md">
              Your personal cloud storage solution. Store, sync, and share your files seamlessly across all devices.
            </p>
          </div>
        </div>
      </div>

      {/* Right side - Login Form */}
      <div className="flex-1 lg:w-1/2 h-full flex items-center justify-center p-6 bg-background">
        <div className="w-full max-w-md">
          {/* Mobile logo */}
          <div className="lg:hidden flex items-center justify-center mb-8">
            <Cloud className="h-10 w-10 mr-3 text-primary" />
            <h1 className="text-3xl font-bold text-primary">Cloudy</h1>
          </div>

          <Card className="shadow-card border-0 bg-card/50 backdrop-blur-sm">
            <CardHeader className="text-center">
              <CardTitle className="text-2xl font-bold text-foreground">
                {isLogin ? "Welcome back" : "Create account"}
              </CardTitle>
              <CardDescription className="text-muted-foreground">
                {isLogin 
                  ? "Sign in to access your cloud storage" 
                  : "Sign up to start using Cloudy"
                }
              </CardDescription>
            </CardHeader>

            <CardContent>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="email" className="text-foreground font-medium">
                    Email address
                  </Label>
                  <div className="relative">
                    <Mail className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                    <Input
                      id="email"
                      name="email"
                      type="email"
                      placeholder="Enter your email"
                      value={formData.email}
                      onChange={handleInputChange}
                      className="pl-10 bg-input border-border focus:ring-primary"
                      required
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="password" className="text-foreground font-medium">
                    Password
                  </Label>
                  <div className="relative">
                    <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                    <Input
                      id="password"
                      name="password"
                      type={showPassword ? "text" : "password"}
                      placeholder="Enter your password"
                      value={formData.password}
                      onChange={handleInputChange}
                      className="pl-10 pr-10 bg-input border-border focus:ring-primary"
                      required
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-3 top-3 text-muted-foreground hover:text-foreground transition-colors"
                    >
                      {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                    </button>
                  </div>
                </div>

                {!isLogin && (
                  <div className="space-y-2">
                    <Label htmlFor="confirmPassword" className="text-foreground font-medium">
                      Confirm Password
                    </Label>
                    <div className="relative">
                      <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                      <Input
                        id="confirmPassword"
                        name="confirmPassword"
                        type={showPassword ? "text" : "password"}
                        placeholder="Confirm your password"
                        value={formData.confirmPassword}
                        onChange={handleInputChange}
                        className="pl-10 bg-input border-border focus:ring-primary"
                        required
                      />
                    </div>
                  </div>
                )}

                {isLogin && (
                  <div className="text-right">
                    <button
                      type="button"
                      onClick={() => setShowForgot(true)}
                      className="text-sm text-primary hover:text-primary/80 transition-colors"
                    >
                      Forgot password?
                    </button>
                  </div>
                )}

                {error && (
                  <div className="text-sm text-red-500" role="alert">
                    {error}
                  </div>
                )}
                <Button
                  type="submit"
                  variant="cloud"
                  className="w-full text-base py-6"
                  disabled={isLoading}
                >
                  {isLoading ? "Signing in..." : isLogin ? "Sign in" : "Create account"}
                </Button>
              </form>
            </CardContent>

            <CardFooter className="flex flex-col space-y-4">
              <Separator className="bg-border" />
              <div className="text-center text-sm text-muted-foreground">
                {isLogin ? "Don't have an account?" : "Already have an account?"}
                <button
                  onClick={() => setIsLogin(!isLogin)}
                  className="ml-1 text-primary hover:text-primary/80 font-medium transition-colors"
                >
                  {isLogin ? "Sign up" : "Sign in"}
                </button>
              </div>
            </CardFooter>
          </Card>

          <div className="mt-6 text-center text-xs text-muted-foreground">
            By continuing, you agree to our Terms- haha just kidding
          </div>
        </div>
      </div>
      {showForgot && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-md">
            <Card className="shadow-card border-0 bg-card/80 backdrop-blur-sm relative">
              <button
                type="button"
                onClick={() => setShowForgot(false)}
                aria-label="Close"
                className="absolute right-4 top-4 text-muted-foreground hover:text-foreground transition-colors"
              >
                <X className="h-5 w-5" />
              </button>
              <CardHeader className="text-center">
                <CardTitle className="text-2xl font-bold text-foreground">Reset password</CardTitle>
                <CardDescription className="text-muted-foreground">
                  Enter your email and we’ll send you a reset link.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <form onSubmit={handleForgotSubmit} className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="forgot-email" className="text-foreground font-medium">
                      Email address
                    </Label>
                    <div className="relative">
                      <Mail className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                      <Input
                        id="forgot-email"
                        name="forgot-email"
                        type="email"
                        placeholder="Enter your email"
                        value={forgotEmail}
                        onChange={(e) => setForgotEmail(e.target.value)}
                        className="pl-10 bg-input border-border focus:ring-primary"
                        required
                      />
                    </div>
                  </div>
                  <Button type="submit" variant="cloud" className="w-full text-base py-6">
                    Send reset link
                  </Button>
                </form>
              </CardContent>
              <CardFooter className="flex justify-center">
                <button
                  type="button"
                  onClick={() => setShowForgot(false)}
                  className="text-sm text-primary hover:text-primary/80 transition-colors"
                >
                  Back to sign in
                </button>
              </CardFooter>
            </Card>
          </div>
        </div>
      )}
    </div>
  );
};

export default LoginPage;